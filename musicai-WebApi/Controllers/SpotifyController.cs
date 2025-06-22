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

    public SpotifyController(IPlaylistServico playlistServico, ISpotifyServico spotifyServico)
    {
        _playlistServico = playlistServico;
        _spotifyServico = spotifyServico;
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
            return StatusCode(500, $"Erro ao gerar playlist: {ex.Message}");
        }
    }

    [HttpPost("gerar-e-salvar-playlist")]
    public async Task<IActionResult> GerarESalvarPlaylist([FromBody] GerarESalvarRequest request)
    {
        if (string.IsNullOrEmpty(request.Mensagem))
            return BadRequest("A mensagem não pode estar vazia");

        if (string.IsNullOrEmpty(request.NomePlaylist))
            return BadRequest("O nome da playlist não pode estar vazio");

        if (string.IsNullOrEmpty(request.AccessToken))
            return BadRequest("O token de acesso não pode estar vazio");

        try
        {
            // Gera a playlist com base na mensagem
            var musicas = await _playlistServico.GerarPlaylistPorMensagem(
                request.Mensagem, 
                request.NumeroDeMusicas ?? 20, 
                request.NomePlaylist);

            if (!musicas.Any())
                return NotFound("Não foi possível encontrar músicas adequadas para a mensagem");

            // Extrai os IDs das músicas para criar a playlist no Spotify
            var musicaIds = musicas.Select(m => m.Id).ToList();
            
            // Cria a playlist no Spotify
            var resultado = await _spotifyServico.CriarPlaylistUsuarioAsync(
                request.NomePlaylist,
                request.Descricao ?? $"Playlist gerada para: {request.Mensagem}",
                musicaIds,
                request.AccessToken);

            if (resultado)
            {
                return Ok(new 
                { 
                    Sucesso = true, 
                    Mensagem = "Playlist criada com sucesso no Spotify!",
                    Musicas = musicas
                });
            }
            else
            {
                return StatusCode(500, "Não foi possível criar a playlist no Spotify");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Token"))
        {
            return Unauthorized("Token de acesso inválido ou expirado. Por favor, autentique-se novamente.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao gerar ou salvar playlist: {ex.Message}");
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
            return StatusCode(500, $"Erro ao obter playlists: {ex.Message}");
        }
    }

    public class GerarESalvarRequest
    {
        public string Mensagem { get; set; } = string.Empty;
        public string NomePlaylist { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int? NumeroDeMusicas { get; set; }
        public string AccessToken { get; set; } = string.Empty;
    }
}