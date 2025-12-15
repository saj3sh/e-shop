using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EShop.Application.Auth;

namespace EShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] RegisterCommandHandler handler,
        CancellationToken ct)
    {
        var command = new RegisterCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.ShippingAddress,
            request.ShippingCity,
            request.ShippingCountry,
            request.BillingAddress,
            request.BillingCity,
            request.BillingCountry
        );
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new
        {
            userId = result.Value!.UserId,
            email = result.Value.Email,
            message = "registration successful, please login"
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginCommandHandler handler,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email);
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND") // password not used
                return Unauthorized(new { error = result.Error, code = result.ErrorCode });

            return BadRequest(new { error = result.Error });
        }

        // set refresh token in http-only cookie
        Response.Cookies.Append("refreshToken", result.Value!.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // set true in production
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new
        {
            accessToken = result.Value.AccessToken,
            role = result.Value.Role.ToString(),
            userId = result.Value.UserId
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromServices] RefreshTokenCommandHandler handler,
        CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Unauthorized(new { error = "refresh token missing" });

        var command = new RefreshTokenCommand(refreshToken);
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });

        Response.Cookies.Append("refreshToken", result.Value!.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new { accessToken = result.Value.AccessToken });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromServices] LogoutCommandHandler handler,
        CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            var command = new LogoutCommand(refreshToken);
            await handler.HandleAsync(command, ct);
        }

        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}

public record RegisterRequest(
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    string ShippingAddress,
    string ShippingCity,
    string ShippingCountry,
    string BillingAddress,
    string BillingCity,
    string BillingCountry
);

public record LoginRequest(string Email);
