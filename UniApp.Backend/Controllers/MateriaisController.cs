using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniApp.Api.Data;
using UniApp.Api.Models;

namespace UniApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MateriaisController : ControllerBase
{
    private readonly AppDbContext _context;

    public MateriaisController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetMateriais([FromQuery] string? categoria, [FromQuery] string? busca)
    {
        var query = _context.Materiais.Include(m => m.Usuario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoria) && categoria != "Todos")
            query = query.Where(m => m.Categoria == categoria);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(m => m.Titulo.Contains(busca) || m.CursoRelacionado.Contains(busca));

        var lista = await query.OrderByDescending(m => m.DataCriacao).ToListAsync();
        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> CriarMaterial([FromBody] Material material)
    {
        material.DataCriacao = DateTime.UtcNow;
        if (material.Modalidade == "Doação") material.Preco = 0;

        _context.Materiais.Add(material);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMateriais), new { id = material.Id }, material);
    }
}
