using Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;
using Servico.Servicos;

namespace musicai.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicAIController(OpenIA openIAService) : ControllerBase
{
    private readonly OpenIA _openIAService = openIAService;

    [HttpPost("enviar-mensagem")]
    public async Task<IActionResult> ProcessarMensagem([FromBody] MensagemRequest request)
    {
        if (string.IsNullOrEmpty(request.Mensagem))
        {
            return BadRequest("A mensagem não pode estar vazia");
        }

        var resposta = await _openIAService.GerarPlaylist(request.Mensagem);
        return Ok(new { resposta });
    }
}