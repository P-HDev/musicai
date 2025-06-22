using System.Text.Json.Serialization;

namespace Dominio.Entidades;

public class AutenticacaoSpotify
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public DateTime DataExpiracao { get; set; }
    public string Scope { get; set; } = string.Empty;
    
    [JsonIgnore]
    public bool EstaExpirado => DateTime.UtcNow.AddMinutes(5) >= DataExpiracao;
}
