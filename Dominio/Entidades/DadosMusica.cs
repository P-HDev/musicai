namespace Dominio.Entidades;

public class DadosMusica
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Artista { get; set; } = string.Empty;
    public string ImagemUrl { get; set; } = string.Empty;
    public string LinkSpotify { get; set; } = string.Empty;
    public string? NomePlaylist { get; set; }
}