using Dominio.Entidades;
using Dominio.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace musicai.Controllers;

[ApiController]
[Route("[controller]")]
public class AutenticacaoSpotifyController : ControllerBase
{
    private readonly ISpotifyServico _spotifyServico;
    private readonly ILogger<AutenticacaoSpotifyController> _logger;
    private readonly string _frontendUrl;

    public AutenticacaoSpotifyController(ISpotifyServico spotifyServico, ILogger<AutenticacaoSpotifyController> logger, IConfiguration configuration)
    {
        _spotifyServico = spotifyServico;
        _logger = logger;
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:5173";
    }

    [HttpGet("autorizar")]
    public IActionResult ObterUrlAutorizacao()
    {
        try 
        {
            var url = _spotifyServico.ObterUrlAutorizacao();
            _logger.LogInformation("Redirecionando para URL de autorização do Spotify: {Url}", url);
            
            Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            
            return Redirect(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar URL de autorização: {Mensagem}", ex.Message);
            return StatusCode(500, "Erro ao gerar URL de autorização. Verifique os logs para mais detalhes.");
        }
    }

    [HttpGet("callback")]
    public async Task<IActionResult> CallbackAutorizacao([FromQuery] string? code = null, [FromQuery] string? error = null, [FromQuery] string? state = null)
    {
        _logger.LogInformation("Callback recebido - Code: {Code}, Error: {Error}, State: {State}", 
            code ?? "null", error ?? "null", state ?? "null");

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("Erro durante autorização do Spotify: {Error}", error);
            return Redirect($"{_frontendUrl}/callback?erro={Uri.EscapeDataString(error)}");
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogError("Código de autorização não fornecido");
            return Redirect($"{_frontendUrl}/callback?erro=codigo-nao-fornecido");
        }

        try
        {
            _logger.LogInformation("Obtendo token com o código fornecido");
            var autenticacao = await _spotifyServico.ObterTokenUsuarioAsync(code);
            
            return Redirect($"{_frontendUrl}/callback" +
                $"?access_token={Uri.EscapeDataString(autenticacao.AccessToken)}" +
                $"&refresh_token={Uri.EscapeDataString(autenticacao.RefreshToken)}" +
                $"&expires_in={autenticacao.ExpiresIn}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar callback de autenticação: {Mensagem}", ex.Message);
            return Redirect($"{_frontendUrl}/callback?erro={Uri.EscapeDataString(ex.Message)}");
        }
    }

    [HttpPost("atualizar-token")]
    public async Task<IActionResult> AtualizarToken([FromBody] AtualizarTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest("Token de atualização não fornecido");
        }

        try
        {
            var autenticacao = await _spotifyServico.AtualizarTokenUsuarioAsync(request.RefreshToken);
            return Ok(autenticacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar token: {Message}", ex.Message);
            return StatusCode(500, $"Erro ao atualizar token: {ex.Message}");
        }
    }

    [HttpPost("logout")]
    public IActionResult RealizarLogout()
    {
        try
        {
            _logger.LogInformation("Realizando logout do usuário");
            
            Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            
            return Ok(new { Mensagem = "Logout realizado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar logout: {Mensagem}", ex.Message);
            return StatusCode(500, "Erro ao realizar logout. Verifique os logs para mais detalhes.");
        }
    }

    public class AtualizarTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
