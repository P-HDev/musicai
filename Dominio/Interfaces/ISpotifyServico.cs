using Dominio.Entidades;

namespace Dominio.Interfaces;

public interface ISpotifyServico
{
    bool TokenEstaValido();
    Task<IEnumerable<DadosMusica>> PesquisarMusicasPorNomesAsync(List<string> nomesMusicas);
}