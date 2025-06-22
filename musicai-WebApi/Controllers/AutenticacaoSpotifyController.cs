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
            
            // Garantindo que estamos respondendo adequadamente para HTTPS
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
    
    [HttpGet("obter-url")]
    public IActionResult ObterApenasUrlAutorizacao()
    {
        var url = _spotifyServico.ObterUrlAutorizacao();
        return Ok(new { UrlAutorizacao = url });
    }

    [HttpGet("callback")]
    public async Task<IActionResult> CallbackAutorizacao([FromQuery] string? code = null, [FromQuery] string? error = null, [FromQuery] string? state = null)
    {
        _logger.LogInformation("Callback recebido - Code: {Code}, Error: {Error}, State: {State}", 
            code ?? "null", error ?? "null", state ?? "null");

        // Log da origem da requisição para diagnóstico de CORS
        _logger.LogInformation("Origem da requisição: {Origin}", 
            Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : "Sem origem");

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
            
            // Redireciona para o frontend com os tokens
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
            return BadRequest("Token de atualização não fornecido");

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

    [HttpPost("criar-playlist")]
    public async Task<IActionResult> CriarPlaylist([FromBody] CriarPlaylistRequest request)
    {
        if (string.IsNullOrEmpty(request.NomePlaylist))
            return BadRequest("Nome da playlist não fornecido");

        if (request.TrackIds == null || !request.TrackIds.Any())
            return BadRequest("Lista de músicas não fornecida");

        if (string.IsNullOrEmpty(request.AccessToken))
            return BadRequest("Token de acesso não fornecido");

        try
        {
            var resultado = await _spotifyServico.CriarPlaylistUsuarioAsync(
                request.NomePlaylist, 
                request.Descricao ?? "Playlist criada com MusicAI", 
                request.TrackIds,
                request.AccessToken);

            if (resultado)
                return Ok(new { Sucesso = true, Mensagem = "Playlist criada com sucesso!" });
            else
                return StatusCode(500, "Não foi possível criar a playlist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar playlist: {Message}", ex.Message);
            return StatusCode(500, $"Erro ao criar playlist: {ex.Message}");
        }
    }

    [HttpGet("url-formatada")]
    public IActionResult ObterUrlAutorizacaoFormatadaExplicitamente()
    {
        try 
        {
            // Construindo a URL manualmente para garantir o formato correto
            string clientId = _spotifyServico.ObterClientId();
            
            // Utiliza o URI exato codificado para URL
            string redirectUriEncoded = Uri.EscapeDataString("http://127.0.0.1:5102/AutenticacaoSpotify/callback");
            
            string scopes = "user-read-private user-read-email playlist-modify-public playlist-modify-private playlist-read-private playlist-read-collaborative";
            string scopesEncoded = Uri.EscapeDataString(scopes);
            
            string url = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUriEncoded}&scope={scopesEncoded}";
            
            _logger.LogInformation("URL de autorização formatada manualmente: {Url}", url);
            
            return Ok(new { UrlAutorizacao = url, UrlParaAcesso = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar URL de autorização formatada: {Mensagem}", ex.Message);
            return StatusCode(500, "Erro ao gerar URL de autorização formatada.");
        }
    }

    public class AtualizarTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class CriarPlaylistRequest
    {
        public string NomePlaylist { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public List<string> TrackIds { get; set; } = new();
        public string AccessToken { get; set; } = string.Empty;
    }
}
