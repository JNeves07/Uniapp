using Microsoft.AspNetCore.Mvc;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Application.Interfaces;

namespace UniApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MateriaisController(IMaterialService materiais) : ControllerBase
{
    /// <summary>Lista anuncios com filtros opcionais por categoria e texto.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMateriais([FromQuery] string? categoria, [FromQuery] string? busca, CancellationToken cancellationToken)
        => Ok(await materiais.ListAsync(categoria, busca, cancellationToken));

    /// <summary>Publica um novo anuncio.</summary>
    [HttpPost]
    public async Task<IActionResult> CriarMaterial([FromBody] CriarMaterialRequest request, CancellationToken cancellationToken)
    {
        var material = await materiais.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMateriais), new { id = material.Id }, material);
    }
}
