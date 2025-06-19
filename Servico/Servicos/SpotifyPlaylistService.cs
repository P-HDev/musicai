using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Servico;

public class SpotifyPlaylistService
{
    public async Task<string> GetUserIdAsync(string accessToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.GetAsync("https://api.spotify.com/v1/me");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        return data.GetProperty("id").GetString();
    }

    public async Task<string> CreatePlaylistAsync(string accessToken, string userId, string playlistName)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var payload = new
        {
            name = playlistName,
            description = "Playlist criada com .NET",
            @public = false
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"https://api.spotify.com/v1/users/{userId}/playlists", content);
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        return data.GetProperty("id").GetString();
    }
}