using Microsoft.AspNetCore.Mvc;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Application.Interfaces;

namespace UniApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GruposController(IGrupoService grupos) : ControllerBase
{
    /// <summary>Lista grupos com filtros opcionais por formato e texto.</summary>
    [HttpGet]
    public async Task<IActionResult> GetGrupos([FromQuery] string? formato, [FromQuery] string? busca, CancellationToken cancellationToken)
        => Ok(await grupos.ListAsync(formato, busca, cancellationToken));

    /// <summary>Cria um grupo de estudo.</summary>
    [HttpPost]
    public async Task<IActionResult> CriarGrupo([FromBody] CriarGrupoRequest request, CancellationToken cancellationToken)
    {
        var grupo = await grupos.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetGrupos), new { id = grupo.Id }, grupo);
    }

    /// <summary>Reserva uma vaga no grupo.</summary>
    [HttpPost("{id:int}/participar")]
    public async Task<IActionResult> Participar(int id, CancellationToken cancellationToken)
        => Ok(await grupos.ParticiparAsync(id, cancellationToken));
}
