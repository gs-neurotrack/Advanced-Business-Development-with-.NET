using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NeuroTrack.DTOs;
using NeuroTrack.DTOs.Hypermedia;
using NeuroTrack.Repositories;
using NeuroTrack.Services.Interfaces;

namespace NeuroTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GsPredictionsController : ControllerBase
{
    private readonly IGsPredictionsServices _service;
    private readonly LinkGenerator _links;

    public GsPredictionsController(IGsPredictionsServices service, LinkGenerator links)
    {
        _service = service;
        _links = links;
    }

    private string Href(string actionName, object? values = null) =>
        _links.GetPathByAction(HttpContext, action: actionName, controller: "GsPredictions", values: values) ?? "#";

    private IEnumerable<Link> PredictionItemLinks(long id) => new[]
    {
        new Link { Rel = "self",   Href = Href(nameof(GetById), new { id }), Method = "GET" },
        new Link { Rel = "delete", Href = Href(nameof(Delete),  new { id }), Method = "DELETE" },
        new Link { Rel = "list",   Href = Href(nameof(GetAll)),             Method = "GET" },
        new Link { Rel = "search", Href = Href(nameof(Search)),             Method = "GET" },
        new Link { Rel = "create", Href = Href(nameof(Add)),                Method = "POST" },
        new Link { Rel = "update", Href = Href(nameof(Update)),             Method = "PUT" }
    };

    private IEnumerable<Link> PredictionCollectionLinks(int page, int pageSize, int totalPages, object? filters = null)
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
    /// Retorna todas as previsões de estresse registradas.
    /// </summary>
    /// <remarks>
    /// Endpoint não paginado. Retorna a coleção completa de previsões com links HATEOAS.
    ///
    /// Status possíveis:
    /// - 200 OK: coleção retornada com sucesso
    /// - 500 Internal Server Error: erro inesperado ao buscar os dados
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(CollectionResource<GsPredictionsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var predictions = await _service.GetAllAsync();

            var items = predictions.Select(p => new Resource<GsPredictionsDTO>
            {
                Data = p,
                _links = PredictionItemLinks(p.IdPrediction)
            });

            var collection = new CollectionResource<GsPredictionsDTO>
            {
                Items = items,
                PageInfo = new
                {
                    page = 1,
                    pageSize = predictions.Count(),
                    totalItems = predictions.Count(),
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
            return StatusCode(500, new { message = "Error retrieving predictions", details = ex.Message });
        }
    }

    // =============================================================
    // GET BY ID
    // =============================================================

    /// <summary>
    /// Obtém uma previsão específica pelo ID.
    /// </summary>
    /// <param name="id">ID da previsão.</param>
    /// <remarks>
    /// Status possíveis:
    /// - 200 OK: previsão encontrada e retornada
    /// - 404 Not Found: nenhuma previsão com o ID informado foi encontrada
    /// - 500 Internal Server Error: erro inesperado no servidor
    /// </remarks>
    /// <returns>Recurso de previsão com links HATEOAS.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Resource<GsPredictionsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var prediction = await _service.GetByIdAsync(id);

            var resource = new Resource<GsPredictionsDTO>
            {
                Data = prediction!,
                _links = PredictionItemLinks(id)
            };

            return Ok(resource);
        }
        catch (GsPredictionsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "PredictionNotFound",
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
    /// Adiciona uma nova previsão de estresse.
    /// </summary>
    /// <param name="dto">Dados da previsão a ser criada.</param>
    /// <remarks>
    /// Campos gerados automaticamente:
    /// - <c>idPrediction</c>: gerado pelo banco (IDENTITY).
    /// - <c>datePredicted</c>: normalmente definido pela aplicação no momento da geração da previsão.
    ///
    /// O cliente precisa enviar apenas os campos de negócio:
    /// <code>
    /// {
    ///   "stressPredicted": 0.82,
    ///   "message": "Risco alto de estresse nas próximas horas.",
    ///   "idUser": 12,
    ///   "idScores": 34,
    ///   "idStatusRisk": 3
    /// }
    /// </code>
    ///
    /// Status possíveis:
    /// - 201 Created: previsão criada com sucesso
    /// - 400 Bad Request: payload inválido ou inconsistências de validação
    /// - 500 Internal Server Error: erro inesperado ao criar o registro
    /// </remarks>
    /// <returns>Recurso criado com links HATEOAS.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Resource<GsPredictionsDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] GsPredictionsDTO dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Payload inválido." });

        var created = await _service.AddAsync(dto);

        var resource = new Resource<GsPredictionsDTO>
        {
            Data = created,
            _links = PredictionItemLinks(created.IdPrediction)
        };

        return CreatedAtAction(nameof(GetById), new { id = created.IdPrediction }, resource);
    }

    // =============================================================
    // PUT UPDATE
    // =============================================================

    /// <summary>
    /// Atualiza uma previsão existente.
    /// </summary>
    /// <param name="dto">
    /// Dados atualizados da previsão. O campo <c>idPrediction</c> deve conter o ID do registro a ser atualizado.
    /// </param>
    /// <remarks>
    /// Exemplo de corpo (JSON):
    /// <code>
    /// {
    ///   "idPrediction": 5,
    ///   "stressPredicted": 0.75,
    ///   "message": "Risco moderado de estresse.",
    ///   "datePredicted": "2025-11-28T10:00:00Z",
    ///   "idUser": 12,
    ///   "idScores": 34,
    ///   "idStatusRisk": 2
    /// }
    /// </code>
    ///
    /// Status possíveis:
    /// - 200 OK: previsão atualizada com sucesso
    /// - 400 Bad Request: payload inválido
    /// - 404 Not Found: previsão com o ID informado não existe
    /// - 500 Internal Server Error: erro inesperado ao atualizar o registro
    /// </remarks>
    /// <returns>Mensagem de sucesso e links HATEOAS.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody] GsPredictionsDTO dto)
    {
        try
        {
            await _service.UpdateAsync(dto);

            return Ok(new
            {
                message = "Prediction updated successfully.",
                _links = PredictionItemLinks(dto.IdPrediction)
            });
        }
        catch (GsPredictionsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "PredictionNotFound",
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
    /// Deleta uma previsão pelo ID.
    /// </summary>
    /// <param name="id">ID da previsão a ser deletada.</param>
    /// <remarks>
    /// Status possíveis:
    /// - 200 OK: previsão deletada com sucesso
    /// - 404 Not Found: previsão com o ID informado não existe
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
                message = $"Prediction with id {id} deleted successfully.",
                _links = new[]
                {
                    new Link { Rel = "list",   Href = Href(nameof(GetAll)), Method = "GET" },
                    new Link { Rel = "search", Href = Href(nameof(Search)), Method = "GET" },
                    new Link { Rel = "create", Href = Href(nameof(Add)),    Method = "POST" }
                }
            });
        }
        catch (GsPredictionsRepository.NotFoundException ex)
        {
            return NotFound(new
            {
                StatusCode = 404,
                ErrorType = "PredictionNotFound",
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
    /// Busca previsões com filtros, paginação e ordenação.
    /// </summary>
    /// <param name="idPrediction">ID exato da previsão para filtro opcional.</param>
    /// <param name="datePredicted">Data da previsão para filtro opcional.</param>
    /// <param name="idUser">ID do usuário para filtro opcional.</param>
    /// <param name="idScores">ID do score associado para filtro opcional.</param>
    /// <param name="idStatusRisk">ID do status de risco para filtro opcional.</param>
    /// <param name="page">Número da página (padrão 1, mínimo 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão 10, máximo 100).</param>
    /// <param name="sortBy">Campo de ordenação (padrão "idPrediction").</param>
    /// <param name="sortDir">Direção da ordenação: <c>asc</c> ou <c>desc</c>.</param>
    /// <remarks>
    /// Exemplo de chamada:
    /// <code>
    /// GET /api/GsPredictions/search?idUser=12&amp;page=1&amp;pageSize=5&amp;sortBy=datePredicted&amp;sortDir=desc
    /// </code>
    ///
    /// Status possíveis:
    /// - 200 OK: resultados retornados com sucesso (podem estar vazios)
    /// - 400 Bad Request: parâmetros de paginação/ordenação inválidos
    /// - 500 Internal Server Error: erro inesperado ao realizar a busca
    /// </remarks>
    /// <returns>Coleção paginada de previsões com links HATEOAS.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(CollectionResource<GsPredictionsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] long? idPrediction,
        [FromQuery] DateTime? datePredicted,
        [FromQuery] long? idUser,
        [FromQuery] long? idScores,
        [FromQuery] long? idStatusRisk,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "idPrediction",
        [FromQuery] string sortDir = "asc")
    {
        try
        {
            var result = await _service.SearchAsync(
                idPrediction, datePredicted, idUser, idScores, idStatusRisk,
                page, pageSize, sortBy, sortDir
            );

            var items = result.Items.Select(p => new Resource<GsPredictionsDTO>
            {
                Data = p,
                _links = PredictionItemLinks(p.IdPrediction)
            });

            var collection = new CollectionResource<GsPredictionsDTO>
            {
                Items = items,
                PageInfo = result.PageInfo,
                _links = PredictionCollectionLinks(
                    page: result.PageInfo.Page,
                    pageSize: result.PageInfo.PageSize,
                    totalPages: result.PageInfo.TotalPages,
                    filters: new { idPrediction, datePredicted, idUser, idScores, idStatusRisk, sortBy, sortDir }
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
            return StatusCode(500, new { message = "Error searching predictions.", details = ex.Message });
        }
    }
}
