using System.Text.Json.Serialization;

namespace Dominio.Entidades;

public class PlaylistSpotify
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ImagemUrl { get; set; } = string.Empty;
    public int TotalFaixas { get; set; }
    public bool Publica { get; set; }
    public bool Colaborativa { get; set; }
    public string UrlExterna { get; set; } = string.Empty;
}
