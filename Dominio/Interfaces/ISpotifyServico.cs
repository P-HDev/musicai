using Dominio.Entidades;

namespace Dominio.Interfaces;

public interface ISpotifyServico
{
    bool TokenEstaValido();
    Task<IEnumerable<DadosMusica>> PesquisarMusicasPorNomesAsync(List<string> nomesMusicas);
    
    // Novos métodos para autenticação OAuth
    string ObterUrlAutorizacao();
    Task<AutenticacaoSpotify> ObterTokenUsuarioAsync(string codigo);
    Task<AutenticacaoSpotify> AtualizarTokenUsuarioAsync(string refreshToken);
    Task<bool> CriarPlaylistUsuarioAsync(string nomePlaylist, string descricao, List<string> trackIds, string accessToken);
    string ObterClientId();
    
    // Método para obter playlists do usuário
    Task<IEnumerable<PlaylistSpotify>> ObterPlaylistsUsuarioAsync(string accessToken);
}