using Dominio.Entidades;
using Dominio.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace musicai.Controllers;

[ApiController]
[Route("[controller]")]
public class SpotifyController(IPlaylistServico playlistServico) : ControllerBase
{
    [HttpPost("gerar-playlist")]
    public async Task<IActionResult> GerarPlaylist([FromBody] MensagemRequest request)
    {
        if (string.IsNullOrEmpty(request.Mensagem))
            return BadRequest("A mensagem não pode estar vazia");

        try
        {
            var musicas = await playlistServico.GerarPlaylistPorMensagem(request.Mensagem);
            return Ok(musicas);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Token de acesso inválido"))
        {
            return Unauthorized("Token de acesso inválido ou expirado. Por favor, autentique-se novamente.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao gerar playlist: {ex.Message}");
        }
    }
}