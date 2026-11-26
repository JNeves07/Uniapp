using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Application.Interfaces;

namespace UniApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>
    /// Login com e-mail institucional (.edu.br).
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => Ok(await auth.LoginAsync(request, cancellationToken));

    /// <summary>
    /// Cadastro de novo usuario institucional.
    /// </summary>
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        => Ok(await auth.RegistrarAsync(request, cancellationToken));

    /// <summary>
    /// Emissao de um novo access token a partir de um refresh token valido.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        => Ok(await auth.RefreshAsync(request.RefreshToken, cancellationToken));

    /// <summary>
    /// Endpoint protegido de exemplo: exige um access token JWT valido.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        nome = User.FindFirst(ClaimTypes.Name)?.Value,
        email = User.FindFirst(ClaimTypes.Email)?.Value
    });
}
