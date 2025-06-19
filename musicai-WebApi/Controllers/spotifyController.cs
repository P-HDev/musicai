using Microsoft.AspNetCore.Mvc;
using Servico;

namespace musicai.Controllers;

public class spotifyController(SpotifyAuthService authService, SpotifyPlaylistService playlistService) : ControllerBase
{
    private readonly SpotifyAuthService _authService = authService;
    private readonly SpotifyPlaylistService _playlistService = playlistService;

    [HttpGet("login")]
    public IActionResult Login()
    {
        var loginUrl = _authService.GetLoginUrl();
        
        return Redirect(loginUrl);
    } 
    
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Código de autorização não fornecido.");

        try
        {
            var accessToken = await _authService.GetAccessTokenAsync(code);
            return Ok(new { AccessToken = accessToken });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao obter o token: {ex.Message}");
        }
    }
    
    [HttpPost("create-playlist")]
    public async Task<IActionResult> CreatePlaylist([FromQuery] string accessToken, [FromQuery] string name)
    {
        if (string.IsNullOrEmpty(accessToken))
            return BadRequest("Access token é obrigatório.");

        if (string.IsNullOrEmpty(name))
            return BadRequest("Nome da playlist é obrigatório.");

        try
        {
            var userId = await _playlistService.GetUserIdAsync(accessToken);
            var playlistId = await _playlistService.CreatePlaylistAsync(accessToken, userId, name);

            return Ok(new { PlaylistId = playlistId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao criar playlist: {ex.Message}");
        }
    }
}