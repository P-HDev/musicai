using Dominio.Entidades;
using Dominio.Interfaces;

namespace Servico.Servicos;

public class PlaylistServico(IOpenIAServico openIaServico, ISpotifyServico spotifyServico) : IPlaylistServico
{
    public async Task<IEnumerable<DadosMusica>> GerarPlaylistPorMensagem(string mensagem, int numeroDeMusicas = 20,
        string? nomePlaylist = null)
    {
        if (string.IsNullOrEmpty(mensagem))
            return Array.Empty<DadosMusica>();

        if (!spotifyServico.TokenEstaValido())
            throw new InvalidOperationException("Token de acesso inválido ou expirado");

        try
        {
            var musicasEArtistas = await openIaServico.GerarPlaylist(mensagem, numeroDeMusicas);

            var musicas = await spotifyServico.PesquisarMusicasPorNomesAsync(musicasEArtistas);

            if (!string.IsNullOrEmpty(nomePlaylist))
            {
                return AdicionarNomePlaylistAMusicas(musicas, nomePlaylist);
            }

            return musicas;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao gerar playlist: {ex.Message}", ex);
        }
    }

    private static IEnumerable<DadosMusica> AdicionarNomePlaylistAMusicas(
        IEnumerable<DadosMusica> musicas,
        string nomePlaylist)
    {
        var musicasComPlaylist = musicas.Select(musica => new DadosMusica
        {
            Id = musica.Id,
            Nome = musica.Nome,
            Artista = musica.Artista,
            ImagemUrl = musica.ImagemUrl,
            LinkSpotify = musica.LinkSpotify,
            NomePlaylist = nomePlaylist
        });

        return musicasComPlaylist;
    }
}