using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Application.Products;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromServices] SearchProductsQueryHandler handler,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var query = new SearchProductsQuery(search, page, pageSize);
        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetProductByIdQueryHandler handler,
        CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var result = await handler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
