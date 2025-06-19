using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Servico;

public class SpotifyAuthService
{
    private readonly IConfiguration _config;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;
    
    public SpotifyAuthService(IConfiguration config)
    {
        _config = config;
        _clientId = _config["Spotify:ClientId"];
        _clientSecret = _config["Spotify:ClientSecret"];
        _redirectUri = _config["Spotify:RedirectUri"];
    }
    
    public string GetLoginUrl()
    {
        var scopes = "playlist-modify-public playlist-modify-private";
        return $"https://accounts.spotify.com/authorize?response_type=code" +
               $"&client_id={_clientId}" +
               $"&scope={Uri.EscapeDataString(scopes)}" +
               $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}";
    }
    
    public async Task<string> GetAccessTokenAsync(string code)
    {
        using var client = new HttpClient();
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var postData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _redirectUri)
        });

        var response = await client.PostAsync("https://accounts.spotify.com/api/token", postData);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        return data.GetProperty("access_token").GetString();
    }
}