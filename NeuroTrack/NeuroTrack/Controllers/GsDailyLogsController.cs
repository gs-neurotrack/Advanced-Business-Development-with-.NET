using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NeuroTrack.DTOs.Hypermedia;
using NeuroTrack.DTOs;
using NeuroTrack.Services.Interfaces;
using NeuroTrack.Repositories;

namespace NeuroTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GsDailyLogsController : ControllerBase
{
    private readonly IGsDailyLogsServices _service;
    private readonly LinkGenerator _links;

    public GsDailyLogsController(IGsDailyLogsServices service, LinkGenerator links)
    {
        _service = service;
        _links = links;
    }

    private string Href(string actionName, object? values = null) =>
        _links.GetPathByAction(HttpContext, action: actionName, controller: "GsDailyLogs", values: values) ?? "#";

    private IEnumerable<Link> LogItemLinks(long id) => new[]
    {
        new Link { Rel = "self",   Href = Href(nameof(GetById), new { id }), Method = "GET" },
        new Link { Rel = "delete", Href = Href(nameof(Delete), new { id }), Method = "DELETE" },
        new Link { Rel = "list",   Href = Href(nameof(GetAll)), Method = "GET" },
        new Link { Rel = "search", Href = Href(nameof(Search)), Method = "GET" },
        new Link { Rel = "create", Href = Href(nameof(Add)), Method = "POST" },
        new Link { Rel = "update", Href = Href(nameof(Update)), Method = "PUT" }
    };

    private IEnumerable<Link> LogCollectionLinks(int page, int pageSize, int totalPages, object? filters = null)
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
    /// Retorna todos os logs diários cadastrados.
    /// </summary>
    /// <remarks>
    /// Endpoint não paginado. Retorna a coleção completa de <c>GsDailyLogs</c> com links HATEOAS.
    ///
    /// Status possíveis:
    /// - 200 OK: coleção retornada com sucesso
    /// - 500 Internal Server Error: erro inesperado ao buscar os dados
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(CollectionResource<GsDailyLogsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var logs = await _service.GetAllAsync();

            var items = logs.Select(l => new Resource<GsDailyLogsDTO>
            {
                Data = l,
                _links = LogItemLinks(l.IdLog)
            });

            var collection = new CollectionResource<GsDailyLogsDTO>
            {
                Items = items,
                PageInfo = new
                {
                    page = 1,
                    pageSize = logs.Count(),
                    totalItems = logs.Count(),
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
            return StatusCode(500, new { message = "Error retrieving logs", details = ex.Message });
        }
    }

    // =============================================================
    // GET BY ID
    // =============================================================

    /// <summary>
    /// Obtém um log diário específico pelo ID.
    /// </summary>
    /// <param name="id">ID do log diário.</param>
    /// <remarks>
    /// Status possíveis:
    /// - 200 OK: log encontrado e retornado no corpo da resposta
    /// - 404 Not Found: nenhum log com o ID informado foi encontrado
    /// - 500 Internal Server Error: erro inesperado no servidor
    /// </remarks>
    /// <returns>Recurso <see cref="GsDailyLogsDTO"/> com links HATEOAS.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Resource<GsDailyLogsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var log = await _service.GetByIdAsync(id);

            var resource = new Resource<GsDailyLogsDTO>
            {
                Data = log!,
                _links = LogItemLinks(id)
            };

            return Ok(resource);
        }
        catch (GsDailyLogsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "LogNotFound",
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
    /// Adiciona um novo log diário.
    /// </summary>
    /// <param name="dto">Dados do log diário a ser criado.</param>
    /// <remarks>
    /// Campos gerados automaticamente:
    /// - <c>idLog</c>: gerado pelo banco (IDENTITY).
    /// - <c>logDate</c>: preenchido automaticamente com a data/hora do registro (conforme regra da aplicação/banco).
    ///
    /// Corpo esperado (JSON) — apenas os campos que o cliente precisa enviar:
    /// <code>
    /// {
    ///   "workHours": 8,
    ///   "meetings": 3,
    ///   "idUser": 12
    /// }
    /// </code>
    ///
    /// Status possíveis:
    /// - 201 Created: log criado com sucesso
    /// - 400 Bad Request: payload inválido ou inconsistente
    /// - 500 Internal Server Error: erro inesperado ao criar o registro
    /// </remarks>
    /// <returns>Recurso criado com links HATEOAS.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Resource<GsDailyLogsDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] GsDailyLogsDTO dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Payload inválido." });

        var created = await _service.AddAsync(dto);

        var resource = new Resource<GsDailyLogsDTO>
        {
            Data = created,
            _links = LogItemLinks(created.IdLog)
        };

        return CreatedAtAction(nameof(GetById), new { id = created.IdLog }, resource);
    }

    // =============================================================
    // PUT UPDATE
    // =============================================================

    /// <summary>
    /// Atualiza um log diário existente.
    /// </summary>
    /// <param name="dto">
    /// Dados atualizados do log diário. O campo <c>idLog</c> deve conter o ID do registro que será atualizado.
    /// </param>
    /// <remarks>
    /// Exemplo de corpo (JSON):
    /// <code>
    /// {
    ///   "idLog": 10,
    ///   "workHours": 7,
    ///   "meetings": 2,
    ///   "logDate": "2025-11-28T10:00:00Z",
    ///   "idUser": 12
    /// }
    /// </code>
    ///
    /// Status possíveis:
    /// - 200 OK: log atualizado com sucesso
    /// - 400 Bad Request: payload inválido
    /// - 404 Not Found: log com o ID informado não existe
    /// - 500 Internal Server Error: erro inesperado ao atualizar o registro
    /// </remarks>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody] GsDailyLogsDTO dto)
    {
        try
        {
            await _service.UpdateAsync(dto);

            return Ok(new
            {
                message = "Daily log updated successfully.",
                _links = LogItemLinks(dto.IdLog)
            });
        }
        catch (GsDailyLogsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "LogNotFound",
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
    /// Deleta um log diário pelo ID.
    /// </summary>
    /// <param name="id">ID do log diário a ser deletado.</param>
    /// <remarks>
    /// Status possíveis:
    /// - 200 OK: log deletado com sucesso
    /// - 404 Not Found: log com o ID informado não existe
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
                message = $"Log with id {id} deleted successfully.",
                _links = new[]
                {
                    new Link { Rel = "list",   Href = Href(nameof(GetAll)), Method = "GET" },
                    new Link { Rel = "search", Href = Href(nameof(Search)), Method = "GET" },
                    new Link { Rel = "create", Href = Href(nameof(Add)), Method = "POST" }
                }
            });
        }
        catch (GsDailyLogsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "LogNotFound",
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
    /// Busca logs diários com filtros, paginação e ordenação.
    /// </summary>
    /// <param name="idLog">ID exato do log para filtro opcional.</param>
    /// <param name="workHours">Quantidade de horas trabalhadas para filtro opcional.</param>
    /// <param name="idUser">ID do usuário para filtro opcional.</param>
    /// <param name="page">Número da página (padrão 1, mínimo 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão 10, máximo 100).</param>
    /// <param name="sortBy">Campo de ordenação: <c>idLog</c>, <c>workHours</c> ou <c>idUser</c>.</param>
    /// <param name="sortDir">Direção da ordenação: <c>asc</c> ou <c>desc</c>.</param>
    /// <remarks>
    /// Exemplo de chamada:
    /// <code>
    /// GET /api/GsDailyLogs/search?idUser=12&amp;page=1&amp;pageSize=5&amp;sortBy=idLog&amp;sortDir=asc
    /// </code>
    ///
    /// Status possíveis:
    /// - 200 OK: resultados retornados com sucesso (pode vir lista vazia)
    /// - 400 Bad Request: parâmetros de paginação/ordenção inválidos
    /// - 500 Internal Server Error: erro inesperado ao realizar a busca
    /// </remarks>
    /// <returns>Coleção paginada de logs com links HATEOAS.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(CollectionResource<GsDailyLogsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] long? idLog,
        [FromQuery] int? workHours,
        [FromQuery] long? idUser,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "idLog",
        [FromQuery] string sortDir = "asc")
    {
        try
        {
            var result = await _service.SearchAsync(
                idLog, workHours, idUser, page, pageSize, sortBy, sortDir
            );

            var items = result.Items.Select(l => new Resource<GsDailyLogsDTO>
            {
                Data = l,
                _links = LogItemLinks(l.IdLog)
            });

            var collection = new CollectionResource<GsDailyLogsDTO>
            {
                Items = items,
                PageInfo = result.PageInfo,
                _links = LogCollectionLinks(
                    page: result.PageInfo.Page,
                    pageSize: result.PageInfo.PageSize,
                    totalPages: result.PageInfo.TotalPages,
                    filters: new { idLog, workHours, idUser, sortBy, sortDir }
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
            return StatusCode(500, new { message = "Error searching logs.", details = ex.Message });
        }
    }
}
