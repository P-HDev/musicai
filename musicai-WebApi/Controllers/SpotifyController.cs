using Dominio.Entidades;
using Dominio.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace musicai.Controllers;

[ApiController]
[Route("[controller]")]
public class SpotifyController : ControllerBase
{
    private readonly IPlaylistServico _playlistServico;
    private readonly ISpotifyServico _spotifyServico;
    private readonly ILogger<SpotifyController> _logger;

    public SpotifyController(IPlaylistServico playlistServico, ISpotifyServico spotifyServico, ILogger<SpotifyController> logger)
    {
        _playlistServico = playlistServico;
        _spotifyServico = spotifyServico;
        _logger = logger;
    }

    [HttpPost("gerar-playlist")]
    public async Task<IActionResult> GerarPlaylist([FromBody] MensagemRequest request)
    {
        if (string.IsNullOrEmpty(request.Mensagem))
            return BadRequest("A mensagem não pode estar vazia");

        try
        {
            var musicas = await _playlistServico.GerarPlaylistPorMensagem(request.Mensagem);
            return Ok(musicas);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Token de acesso inválido"))
        {
            return Unauthorized("Token de acesso inválido ou expirado. Por favor, autentique-se novamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar playlist: {Mensagem}", ex.Message);
            return StatusCode(500, $"Erro ao gerar playlist: {ex.Message}");
        }
    }

    [HttpPost("gerar-e-salvar-playlist")]
    public async Task<IActionResult> GerarESalvarPlaylist([FromBody] CriarPlaylistRequest request)
    {
        if (string.IsNullOrEmpty(request.NomePlaylist))
            return BadRequest("O nome da playlist não pode estar vazio");

        if (request.TrackIds == null || !request.TrackIds.Any())
            return BadRequest("A lista de músicas não pode estar vazia");

        if (string.IsNullOrEmpty(request.AccessToken))
            return BadRequest("O token de acesso não pode estar vazio");

        try
        {
            _logger.LogInformation("Criando playlist: {Nome} com {NumTracks} faixas", request.NomePlaylist, request.TrackIds.Count);
            
            // Cria a playlist no Spotify
            var resultado = await _spotifyServico.CriarPlaylistUsuarioAsync(
                request.NomePlaylist,
                request.Descricao ?? "Playlist criada com MusicAI",
                request.TrackIds,
                request.AccessToken);

            if (resultado)
            {
                return Ok(new 
                { 
                    Sucesso = true, 
                    Mensagem = "Playlist criada com sucesso no Spotify!"
                });
            }
            
            return StatusCode(500, "Não foi possível criar a playlist no Spotify");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Token"))
        {
            _logger.LogError(ex, "Erro de token ao criar playlist: {Mensagem}", ex.Message);
            return Unauthorized("Token de acesso inválido ou expirado. Por favor, autentique-se novamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar playlist: {Mensagem}", ex.Message);
            return StatusCode(500, $"Erro ao criar playlist: {ex.Message}");
        }
    }

    [HttpGet("playlists")]
    public async Task<IActionResult> ObterPlaylists([FromHeader(Name = "Authorization")] string autorizacao)
    {
        if (string.IsNullOrEmpty(autorizacao) || !autorizacao.StartsWith("Bearer "))
        {
            return BadRequest("Token de acesso não fornecido ou em formato inválido");
        }

        try
        {
            string accessToken = autorizacao.Substring("Bearer ".Length);
            var playlists = await _spotifyServico.ObterPlaylistsUsuarioAsync(accessToken);
            return Ok(playlists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter playlists: {Mensagem}", ex.Message);
            return StatusCode(500, $"Erro ao obter playlists: {ex.Message}");
        }
    }

    public class CriarPlaylistRequest
    {
        public string NomePlaylist { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public List<string> TrackIds { get; set; } = new();
        public string AccessToken { get; set; } = string.Empty;
    }
}