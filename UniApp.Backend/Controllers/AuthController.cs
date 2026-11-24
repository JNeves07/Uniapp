using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniApp.Api.Data;
using UniApp.Api.Models;

namespace UniApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public record LoginRequest(string Email, string Senha);
    public record RegisterRequest(string Nome, string Email, string Senha, string Curso);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.EmailInstitucional == req.Email && u.SenhaHash == req.Senha);

        if (user == null)
            return Unauthorized(new { mensagem = "E-mail institucional ou senha inválidos." });

        var token = GerarToken(user);
        return Ok(new { token, usuario = new { user.Id, user.Nome, user.EmailInstitucional, user.Curso, user.Reputacao } });
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegisterRequest req)
    {
        if (!req.Email.EndsWith(".edu.br"))
            return BadRequest(new { mensagem = "Utilize um e-mail institucional válido (.edu.br)." });

        if (await _context.Usuarios.AnyAsync(u => u.EmailInstitucional == req.Email))
            return BadRequest(new { mensagem = "E-mail já cadastrado." });

        var novo = new Usuario
        {
            Nome = req.Nome,
            EmailInstitucional = req.Email,
            SenhaHash = req.Senha,
            Curso = req.Curso
        };

        _context.Usuarios.Add(novo);
        await _context.SaveChangesAsync();

        var token = GerarToken(novo);
        return Ok(new { token, usuario = new { novo.Id, novo.Nome, novo.EmailInstitucional, novo.Curso, novo.Reputacao } });
    }

    private string GerarToken(Usuario user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.EmailInstitucional),
            new Claim(ClaimTypes.Name, user.Nome)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
