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

    public AutenticacaoSpotifyController(ISpotifyServico spotifyServico, ILogger<AutenticacaoSpotifyController> logger)
    {
        _spotifyServico = spotifyServico;
        _logger = logger;
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
            return BadRequest($"Erro durante a autorização do Spotify: {error}");
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogError("Código de autorização não fornecido");
            return BadRequest("Código de autorização não fornecido");
        }

        try
        {
            _logger.LogInformation("Obtendo token com o código fornecido");
            var autenticacao = await _spotifyServico.ObterTokenUsuarioAsync(code);
            
            // Criando uma página HTML simples para mostrar o sucesso e os tokens
            var htmlResponse = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Autenticação Spotify - Sucesso</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .container {{ max-width: 800px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
        .success {{ color: green; }}
        .token-info {{ background-color: #f5f5f5; padding: 10px; border-radius: 5px; word-wrap: break-word; }}
        code {{ font-family: monospace; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1 class=""success"">Autenticação concluída com sucesso!</h1>
        <p>A sua conta Spotify foi conectada com sucesso ao MusicAI.</p>
        <h3>Informações do token (guarde em local seguro):</h3>
        <div class=""token-info"">
            <p><strong>Access Token:</strong> <code>{autenticacao.AccessToken[..15]}...</code></p>
            <p><strong>Refresh Token:</strong> <code>{autenticacao.RefreshToken[..15]}...</code></p>
            <p><strong>Expira em:</strong> {autenticacao.ExpiresIn} segundos</p>
        </div>
        <p>Você pode fechar esta janela e retornar ao aplicativo.</p>
    </div>
</body>
</html>";

            _logger.LogInformation("Autenticação bem-sucedida, retornando página HTML de sucesso");
            Response.Headers.Append("Cache-Control", "no-store");
            Response.Headers.Append("Pragma", "no-cache");
            return Content(htmlResponse, "text/html; charset=utf-8");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante autenticação: {Message}", ex.Message);
            
            // Retornando página HTML com o erro para melhor visualização
            var htmlErro = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Autenticação Spotify - Erro</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .container {{ max-width: 800px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
        .error {{ color: red; }}
        .details {{ background-color: #f5f5f5; padding: 10px; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1 class=""error"">Erro na autenticação</h1>
        <p>Ocorreu um erro durante o processo de autenticação com o Spotify:</p>
        <div class=""details"">
            <p><strong>Mensagem:</strong> {ex.Message}</p>
        </div>
        <p>Por favor, tente novamente ou contate o suporte.</p>
    </div>
</body>
</html>";

            Response.Headers.Append("Cache-Control", "no-store");
            Response.Headers.Append("Pragma", "no-cache");
            return Content(htmlErro, "text/html; charset=utf-8");
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
