using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
    [HttpGet]
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
    [HttpGet("{id}")]
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
    [HttpPost]
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
    [HttpPut]
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
    [HttpDelete("{id}")]
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
    [HttpGet("search")]
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
