using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NeuroTrack.DTOs;
using NeuroTrack.DTOs.Hypermedia;
using NeuroTrack.Services.Interfaces;
using NeuroTrack.Repositories;

namespace NeuroTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GsScoresController : ControllerBase
{
    private readonly IGsScoresServices _service;
    private readonly LinkGenerator _links;

    public GsScoresController(IGsScoresServices service, LinkGenerator links)
    {
        _service = service;
        _links = links;
    }

    private string Href(string actionName, object? values = null) =>
        _links.GetPathByAction(HttpContext, action: actionName, controller: "GsScores", values: values) ?? "#";

    private IEnumerable<Link> ScoreItemLinks(long id) => new[]
    {
        new Link { Rel = "self",   Href = Href(nameof(GetById), new { id }), Method = "GET" },
        new Link { Rel = "delete", Href = Href(nameof(Delete),  new { id }), Method = "DELETE" },
        new Link { Rel = "list",   Href = Href(nameof(GetAll)),             Method = "GET" },
        new Link { Rel = "search", Href = Href(nameof(Search)),             Method = "GET" },
        new Link { Rel = "create", Href = Href(nameof(Add)),                Method = "POST" },
        new Link { Rel = "update", Href = Href(nameof(Update)),             Method = "PUT" }
    };

    private IEnumerable<Link> ScoreCollectionLinks(int page, int pageSize, int totalPages, object? filters = null)
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
            new Link { Rel = "create", Href = Href(nameof(Add)),                Method = "POST" },
            new Link { Rel = "all",    Href = Href(nameof(GetAll)),             Method = "GET" }
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
    /// Retorna todos os scores de estresse registrados.
    /// </summary>
    /// <remarks>
    /// Endpoint não paginado. Retorna a coleção completa de scores com links HATEOAS.
    ///
    /// Status possíveis:
    /// - 200 OK: coleção retornada com sucesso.
    /// - 500 Internal Server Error: erro inesperado ao buscar os dados.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(CollectionResource<GsScoresDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var scores = await _service.GetAllAsync();

            var items = scores.Select(s => new Resource<GsScoresDTO>
            {
                Data = s,
                _links = ScoreItemLinks(s.IdScores)
            });

            var collection = new CollectionResource<GsScoresDTO>
            {
                Items = items,
                PageInfo = new
                {
                    page = 1,
                    pageSize = scores.Count(),
                    totalItems = scores.Count(),
                    totalPages = 1
                },
                _links = new[]
                {
                    new Link { Rel = "self",   Href = Href(nameof(GetAll)), Method = "GET" },
                    new Link { Rel = "search", Href = Href(nameof(Search)), Method = "GET" },
                    new Link { Rel = "create", Href = Href(nameof(Add)),    Method = "POST" }
                }
            };

            return Ok(collection);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving scores", details = ex.Message });
        }
    }

    // =============================================================
    // GET BY ID
    // =============================================================

    /// <summary>
    /// Obtém um score específico pelo ID.
    /// </summary>
    /// <param name="id">ID do score.</param>
    /// <remarks>
    /// Status possíveis:
    /// - 200 OK: score encontrado e retornado.
    /// - 404 Not Found: nenhum score com o ID informado foi encontrado.
    /// - 500 Internal Server Error: erro inesperado no servidor.
    /// </remarks>
    /// <returns>Recurso de score com links HATEOAS.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Resource<GsScoresDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var score = await _service.GetByIdAsync(id);

            var resource = new Resource<GsScoresDTO>
            {
                Data = score!,
                _links = ScoreItemLinks(id)
            };

            return Ok(resource);
        }
        catch (GsScoresRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "ScoreNotFound",
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
    /// Adiciona um novo score de estresse.
    /// </summary>
    /// <param name="dto">Dados do score a ser criado.</param>
    /// <remarks>
    /// Campos gerados automaticamente pelo sistema/banco:
    /// - <c>idScores</c>: gerado automaticamente pela tabela (IDENTITY).
    /// - <c>dateScore</c>: normalmente preenchido pela aplicação no momento do cálculo do score.
    /// - <c>createdAt</c>: data de criação do registro.
    ///
    /// No uso típico, o cliente (frontend ou outro serviço) deve enviar apenas os campos de negócio:
    /// <code>
    /// {
    ///   "scoreValue": 0.82,
    ///   "timeRecommendation": 15,
    ///   "idStatusRisk": 2,
    ///   "idUser": 12,
    ///   "idLog": 40
    /// }
    /// </code>
    ///
    /// Status possíveis:
    /// - 201 Created: score criado com sucesso.
    /// - 400 Bad Request: payload inválido ou dados inconsistentes.
    /// - 500 Internal Server Error: erro inesperado ao criar o registro.
    /// </remarks>
    /// <returns>Recurso criado com links HATEOAS.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Resource<GsScoresDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] GsScoresDTO dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Payload inválido." });

        var created = await _service.AddAsync(dto);

        var resource = new Resource<GsScoresDTO>
        {
            Data = created,
            _links = ScoreItemLinks(created.IdScores)
        };

        return CreatedAtAction(nameof(GetById), new { id = created.IdScores }, resource);
    }

    // =============================================================
    // PUT UPDATE
    // =============================================================

    /// <summary>
    /// Atualiza um score existente.
    /// </summary>
    /// <param name="dto">
    /// Dados atualizados do score. O campo <c>idScores</c> deve conter o ID do registro que será atualizado.
    /// </param>
    /// <remarks>
    /// Exemplo de corpo (JSON) para atualização:
    /// <code>
    /// {
    ///   "idScores": 5,
    ///   "dateScore": "2025-11-28T10:00:00Z",
    ///   "scoreValue": 0.76,
    ///   "timeRecommendation": 20,
    ///   "createdAt": "2025-11-28T10:05:00Z",
    ///   "idStatusRisk": 3,
    ///   "idUser": 12,
    ///   "idLog": 40
    /// }
    /// </code>
    ///
    /// Status possíveis:
    /// - 200 OK: score atualizado com sucesso.
    /// - 400 Bad Request: payload inválido.
    /// - 404 Not Found: score com o ID informado não existe.
    /// - 500 Internal Server Error: erro inesperado ao atualizar o registro.
    /// </remarks>
    /// <returns>Mensagem de sucesso e links HATEOAS.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody] GsScoresDTO dto)
    {
        try
        {
            await _service.UpdateAsync(dto);

            return Ok(new
            {
                message = "Score updated successfully.",
                _links = ScoreItemLinks(dto.IdScores)
            });
        }
        catch (GsScoresRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "ScoreNotFound",
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
    /// Deleta um score pelo ID.
    /// </summary>
    /// <param name="id">ID do score a ser deletado.</param>
    /// <remarks>
    /// Status possíveis:
    /// - 200 OK: score deletado com sucesso.
    /// - 404 Not Found: score com o ID informado não existe.
    /// - 500 Internal Server Error: erro inesperado ao excluir o registro.
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
                message = $"Score with id {id} deleted successfully.",
                _links = new[]
                {
                    new Link { Rel = "list",   Href = Href(nameof(GetAll)), Method = "GET" },
                    new Link { Rel = "search", Href = Href(nameof(Search)), Method = "GET" },
                    new Link { Rel = "create", Href = Href(nameof(Add)),    Method = "POST" }
                }
            });
        }
        catch (GsScoresRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "ScoreNotFound",
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
    /// Busca scores com filtros, paginação e ordenação.
    /// </summary>
    /// <param name="idScores">ID exato do score para filtro opcional.</param>
    /// <param name="dateScore">Data em que o score foi calculado para filtro opcional.</param>
    /// <param name="createdAt">Data de criação do registro para filtro opcional.</param>
    /// <param name="idStatusRisk">ID do status de risco associado ao score para filtro opcional.</param>
    /// <param name="idUser">ID do usuário associado ao score para filtro opcional.</param>
    /// <param name="page">Número da página (padrão 1, mínimo 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão 10, máximo 100).</param>
    /// <param name="sortBy">Campo de ordenação (padrão "idScores").</param>
    /// <param name="sortDir">Direção da ordenação: <c>asc</c> ou <c>desc</c>.</param>
    /// <remarks>
    /// Exemplo de chamada:
    /// <code>
    /// GET /api/GsScores/search?idUser=12&amp;page=1&amp;pageSize=5&amp;sortBy=dateScore&amp;sortDir=desc
    /// </code>
    ///
    /// Status possíveis:
    /// - 200 OK: resultados retornados com sucesso (podem estar vazios).
    /// - 400 Bad Request: parâmetros de paginação/ordenação inválidos.
    /// - 500 Internal Server Error: erro inesperado ao realizar a busca.
    /// </remarks>
    /// <returns>Coleção paginada de scores com links HATEOAS.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(CollectionResource<GsScoresDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] long? idScores,
        [FromQuery] DateTime? dateScore,
        [FromQuery] DateTime? createdAt,
        [FromQuery] long? idStatusRisk,
        [FromQuery] long? idUser,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "idScores",
        [FromQuery] string sortDir = "asc")
    {
        try
        {
            var result = await _service.SearchAsync(
                idScores, dateScore, createdAt, idStatusRisk, idUser, page, pageSize, sortBy, sortDir
            );

            var items = result.Items.Select(s => new Resource<GsScoresDTO>
            {
                Data = s,
                _links = ScoreItemLinks(s.IdScores)
            });

            var collection = new CollectionResource<GsScoresDTO>
            {
                Items = items,
                PageInfo = result.PageInfo,
                _links = ScoreCollectionLinks(
                    page: result.PageInfo.Page,
                    pageSize: result.PageInfo.PageSize,
                    totalPages: result.PageInfo.TotalPages,
                    filters: new { idScores, dateScore, createdAt, idStatusRisk, idUser, sortBy, sortDir }
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
            return StatusCode(500, new { message = "Error searching scores.", details = ex.Message });
        }
    }
}
