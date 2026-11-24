using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniApp.Api.Data;
using UniApp.Api.Models;

namespace UniApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GruposController : ControllerBase
{
    private readonly AppDbContext _context;

    public GruposController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetGrupos([FromQuery] string? formato, [FromQuery] string? busca)
    {
        var query = _context.GruposEstudo.AsQueryable();

        if (!string.IsNullOrWhiteSpace(formato) && formato != "Todas")
            query = query.Where(g => g.Formato == formato);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(g => g.Nome.Contains(busca) || g.Disciplina.Contains(busca));

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CriarGrupo([FromBody] GrupoEstudo grupo)
    {
        _context.GruposEstudo.Add(grupo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGrupos), new { id = grupo.Id }, grupo);
    }

    [HttpPost("{id}/participar")]
    public async Task<IActionResult> Participar(int id)
    {
        var grupo = await _context.GruposEstudo.FindAsync(id);
        if (grupo == null) return NotFound();
        if (grupo.VagasDisponiveis <= 0) return BadRequest(new { mensagem = "Grupo lotado." });

        grupo.VagasDisponiveis--;
        await _context.SaveChangesAsync();
        return Ok(grupo);
    }
}
