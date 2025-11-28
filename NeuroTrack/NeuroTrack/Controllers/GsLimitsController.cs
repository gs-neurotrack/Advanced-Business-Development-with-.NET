using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NeuroTrack.DTOs;
using NeuroTrack.DTOs.Hypermedia;
using NeuroTrack.Services.Interfaces;
using NeuroTrack.Repositories;

namespace NeuroTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GsLimitsController : ControllerBase
{
    private readonly IGsLimitsServices _service;
    private readonly LinkGenerator _links;

    public GsLimitsController(IGsLimitsServices service, LinkGenerator links)
    {
        _service = service;
        _links = links;
    }

    private string Href(string actionName, object? values = null) =>
        _links.GetPathByAction(HttpContext, action: actionName, controller: "GsLimits", values: values) ?? "#";

    private IEnumerable<Link> LimitItemLinks(long id) => new[]
    {
        new Link { Rel = "self",   Href = Href(nameof(GetById), new { id }), Method = "GET" },
        new Link { Rel = "delete", Href = Href(nameof(Delete), new { id }), Method = "DELETE" },
        new Link { Rel = "list",   Href = Href(nameof(GetAll)), Method = "GET" },
        new Link { Rel = "search", Href = Href(nameof(Search)), Method = "GET" },
        new Link { Rel = "create", Href = Href(nameof(Add)), Method = "POST" },
        new Link { Rel = "update", Href = Href(nameof(Update)), Method = "PUT" }
    };

    private IEnumerable<Link> LimitCollectionLinks(int page, int pageSize, int totalPages, object? filters = null)
    {
        var baseValues = new Dictionary<string, object?>
        {
            ["page"] = page,
            ["pageSize"] = pageSize
        };

        if (filters is not null)
        {
            foreach (var prop in filters.GetType().GetProperties())
                baseValues[prop.Name] = prop.GetValue(filters);
        }

        var links = new List<Link>
        {
            new Link { Rel = "self",   Href = Href(nameof(Search), baseValues), Method = "GET" },
            new Link { Rel = "create", Href = Href(nameof(Add)), Method = "POST" },
            new Link { Rel = "all",    Href = Href(nameof(GetAll)), Method = "GET" }
        };

        if (page > 1)
        {
            var prev = new Dictionary<string, object?>(baseValues) { ["page"] = page - 1 };
            links.Add(new Link { Rel = "prev", Href = Href(nameof(Search), prev), Method = "GET" });
        }

        if (page < totalPages)
        {
            var next = new Dictionary<string, object?>(baseValues) { ["page"] = page + 1 };
            links.Add(new Link { Rel = "next", Href = Href(nameof(Search), next), Method = "GET" });
        }

        return links;
    }

    // =============================================================
    // GET ALL
    // =============================================================

    /// <summary>
    /// Retorna todos os limites configurados.
    /// </summary>
    /// <remarks>
    /// Endpoint não paginado. Retorna a coleção completa de limites (horas e reuniões) com links HATEOAS.
    ///
    /// Status possíveis:
    /// - 200 OK: coleção retornada com sucesso
    /// - 500 Internal Server Error: erro inesperado ao buscar os dados
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(CollectionResource<GsLimitsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var limits = await _service.GetAllAsync();

            var items = limits.Select(l => new Resource<GsLimitsDTO>
            {
                Data = l,
                _links = LimitItemLinks(l.IdLimits)
            });

            var collection = new CollectionResource<GsLimitsDTO>
            {
                Items = items,
                PageInfo = new
                {
                    page = 1,
                    pageSize = limits.Count(),
                    totalItems = limits.Count(),
                    totalPages = 1
                },
                _links = new[]
                {
                    new Link { Rel = "self",   Href = Href(nameof(GetAll)), Method = "GET" },
                    new Link { Rel = "search", Href = Href(nameof(Search)), Method = "GET" },
                    new Link { Rel = "create", Href = Href(nameof(Add)), Method = "POST" }
                }
            };

            return Ok(collection);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving limits", details = ex.Message });
        }
    }

    // =============================================================
    // GET BY ID
    // =============================================================

    /// <summary>
    /// Obtém um limite específico pelo ID.
    /// </summary>
    /// <param name="id">ID do limite.</param>
    /// <remarks>
    /// Status possíveis:
    /// - 200 OK: limite encontrado e retornado
    /// - 404 Not Found: nenhum limite com o ID informado foi encontrado
    /// - 500 Internal Server Error: erro inesperado no servidor
    /// </remarks>
    /// <returns>Recurso de limite com links HATEOAS.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Resource<GsLimitsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var limit = await _service.GetByIdAsync(id);

            var resource = new Resource<GsLimitsDTO>
            {
                Data = limit!,
                _links = LimitItemLinks(id)
            };

            return Ok(resource);
        }
        catch (GsLimitsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "LimitNotFound",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Unexpected error", details = ex.Message });
        }
    }

    // =============================================================
    // POST CREATE
    // =============================================================

    /// <summary>
    /// Adiciona um novo limite de horas e reuniões.
    /// </summary>
    /// <param name="dto">Dados do limite a ser criado.</param>
    /// <remarks>
    /// Campos gerados automaticamente:
    /// - <c>idLimits</c>: gerado pelo banco (IDENTITY).
    /// - <c>createdAt</c>: registrado automaticamente com a data/hora de criação (regra de aplicação/banco).
    ///
    /// O cliente precisa informar apenas:
    /// <code>
    /// {
    ///   "limitHours": 8,
    ///   "limitMeetings": 5
    /// }
    /// </code>
    ///
    /// Status possíveis:
    /// - 201 Created: limite criado com sucesso
    /// - 400 Bad Request: payload inválido ou inconsistências de validação
    /// - 500 Internal Server Error: erro inesperado ao criar o registro
    /// </remarks>
    /// <returns>Recurso criado com links HATEOAS.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Resource<GsLimitsDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] GsLimitsDTO dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Payload inválido." });

        var created = await _service.AddAsync(dto);

        var resource = new Resource<GsLimitsDTO>
        {
            Data = created,
            _links = LimitItemLinks(created.IdLimits)
        };

        return CreatedAtAction(nameof(GetById), new { id = created.IdLimits }, resource);
    }

    // =============================================================
    // PUT UPDATE
    // =============================================================

    /// <summary>
    /// Atualiza um limite existente.
    /// </summary>
    /// <param name="dto">
    /// Dados atualizados do limite. O campo <c>idLimits</c> deve conter o ID do registro a ser atualizado.
    /// </param>
    /// <remarks>
    /// Exemplo de corpo (JSON):
    /// <code>
    /// {
    ///   "idLimits": 3,
    ///   "limitHours": 10,
    ///   "limitMeetings": 6,
    ///   "createdAt": "2025-11-28T10:00:00Z"
    /// }
    /// </code>
    ///
    /// Observação: <c>createdAt</c> normalmente é controlado pela aplicação/banco; o envio desse campo
    /// pode ser opcional dependendo da sua regra de negócio.
    ///
    /// Status possíveis:
    /// - 200 OK: limite atualizado com sucesso
    /// - 400 Bad Request: payload inválido
    /// - 404 Not Found: limite com o ID informado não existe
    /// - 500 Internal Server Error: erro inesperado ao atualizar o registro
    /// </remarks>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody] GsLimitsDTO dto)
    {
        try
        {
            await _service.UpdateAsync(dto);

            return Ok(new
            {
                message = "Limit updated successfully.",
                _links = LimitItemLinks(dto.IdLimits)
            });
        }
        catch (GsLimitsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "LimitNotFound",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // =============================================================
    // DELETE
    // =============================================================

    /// <summary>
    /// Deleta um limite pelo ID.
    /// </summary>
    /// <param name="id">ID do limite a ser deletado.</param>
    /// <remarks>
    /// Status possíveis:
    /// - 200 OK: limite deletado com sucesso
    /// - 404 Not Found: limite com o ID informado não existe
    /// - 500 Internal Server Error: erro inesperado ao excluir o registro
    /// </remarks>
    /// <returns>Mensagem de confirmação e links para ações relacionadas.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _service.DeleteAsync(id);

            return Ok(new
            {
                message = $"Limit with id {id} deleted successfully.",
                _links = new[]
                {
                    new Link { Rel = "list",   Href = Href(nameof(GetAll)), Method = "GET" },
                    new Link { Rel = "search", Href = Href(nameof(Search)), Method = "GET" },
                    new Link { Rel = "create", Href = Href(nameof(Add)), Method = "POST" }
                }
            });
        }
        catch (GsLimitsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "LimitNotFound",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // =============================================================
    // SEARCH
    // =============================================================

    /// <summary>
    /// Busca limites com filtros, paginação e ordenação.
    /// </summary>
    /// <param name="idLimits">ID exato do limite para filtro opcional.</param>
    /// <param name="limitHours">Quantidade máxima de horas para filtro opcional.</param>
    /// <param name="limitMeetings">Quantidade máxima de reuniões para filtro opcional.</param>
    /// <param name="createdAt">Data de criação para filtro opcional.</param>
    /// <param name="page">Número da página (padrão 1, mínimo 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão 10, máximo 100).</param>
    /// <param name="sortBy">Campo de ordenação (padrão "idLimits").</param>
    /// <param name="sortDir">Direção da ordenação: <c>asc</c> ou <c>desc</c>.</param>
    /// <remarks>
    /// Exemplo de chamada:
    /// <code>
    /// GET /api/GsLimits/search?limitHours=8&amp;page=1&amp;pageSize=5&amp;sortBy=idLimits&amp;sortDir=asc
    /// </code>
    ///
    /// Status possíveis:
    /// - 200 OK: resultados retornados com sucesso (podem estar vazios)
    /// - 400 Bad Request: parâmetros de paginação/ordenação inválidos
    /// - 500 Internal Server Error: erro inesperado ao realizar a busca
    /// </remarks>
    /// <returns>Coleção paginada de limites com links HATEOAS.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(CollectionResource<GsLimitsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] long? idLimits,
        [FromQuery] int? limitHours,
        [FromQuery] int? limitMeetings,
        [FromQuery] DateTime? createdAt,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "idLimits",
        [FromQuery] string sortDir = "asc")
    {
        try
        {
            var result = await _service.SearchAsync(
                idLimits, limitHours, limitMeetings, createdAt, page, pageSize, sortBy, sortDir
            );

            var items = result.Items.Select(l => new Resource<GsLimitsDTO>
            {
                Data = l,
                _links = LimitItemLinks(l.IdLimits)
            });

            var collection = new CollectionResource<GsLimitsDTO>
            {
                Items = items,
                PageInfo = result.PageInfo,
                _links = LimitCollectionLinks(
                    page: result.PageInfo.Page,
                    pageSize: result.PageInfo.PageSize,
                    totalPages: result.PageInfo.TotalPages,
                    filters: new { idLimits, limitHours, limitMeetings, createdAt, sortBy, sortDir }
                )
            };

            return Ok(collection);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error searching limits.", details = ex.Message });
        }
    }
}
