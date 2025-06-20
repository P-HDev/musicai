using Dominio.Entidades;

namespace Dominio.Interfaces;

public interface IPlaylistServico
{
    Task<IEnumerable<DadosMusica>> GerarPlaylistPorMensagem(string mensagem, int numeroDeMusicas = 20, string? nomePlaylist = null);
}
