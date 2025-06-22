using Dominio.Entidades;
using Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;

namespace Servico.Servicos;

public class SpotifyServico : ISpotifyServico
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private ISpotifyClient? _spotifyClient;
    private string? _tokenAtual;
    private DateTime _dataExpiracao = DateTime.MinValue;
    private readonly object _bloqueioToken = new();

    public SpotifyServico(IConfiguration configuracao)
    {
        _clientId = configuracao["SpotifyConfiguracao:ClientId"] ?? 
                throw new ArgumentNullException(nameof(configuracao), "ClientId não configurado");
        _clientSecret = configuracao["SpotifyConfiguracao:ClientSecret"] ?? 
                    throw new ArgumentNullException(nameof(configuracao), "ClientSecret não configurado");
        
        InicializarTokenSincrono();
    }

    public SpotifyServico() : this(null!)
    {
        // Construtor sem parâmetros para compatibilidade com DI se necessário
    }

    private void InicializarTokenSincrono()
    {
        InicializarToken().GetAwaiter().GetResult();
    }

    private async Task InicializarToken()
    {
        await ObterTokenClientCredentialsAsync();
    }

    public bool TokenEstaValido() => 
        _spotifyClient != null && 
        _tokenAtual != null && 
        DateTime.UtcNow.AddSeconds(60) < _dataExpiracao;

    public async Task<IEnumerable<DadosMusica>> PesquisarMusicasPorNomesAsync(List<string> nomesMusicas)
    {
        if (EstaListaVazia(nomesMusicas))
            return Array.Empty<DadosMusica>();

        await AtualizarTokenSeNecessarioAsync();
        
        var resultados = new List<DadosMusica>();
        
        foreach (var nome in nomesMusicas)
        {
            var musica = await TentarObterMusicaPorNome(nome);
            if (musica != null)
                resultados.Add(musica);

            await Task.Delay(100);
        }

        return resultados;
    }
    
    private static bool EstaListaVazia(List<string> lista) => 
        lista == null || !lista.Any();
        
    private async Task<DadosMusica?> TentarObterMusicaPorNome(string nome)
    {
        try
        {
            var musicasEncontradas = await PesquisarMusicaAsync(nome);
            return musicasEncontradas.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar a música '{nome}': {ex.Message}");
            return null;
        }
    }
    
    private async Task<IEnumerable<DadosMusica>> PesquisarMusicaAsync(string termo)
    {
        if (_spotifyClient == null)
            throw new InvalidOperationException("Cliente do Spotify não está inicializado");
        
        var searchRequest = new SearchRequest(SearchRequest.Types.Track, termo) { Limit = 20 };
        var result = await _spotifyClient.Search.Item(searchRequest);
        
        return ConverterParaDadosMusicas(result);
    }

    private static IEnumerable<DadosMusica> ConverterParaDadosMusicas(SearchResponse result)
    {
        if (result.Tracks?.Items == null)
            return Array.Empty<DadosMusica>();
            
        return result.Tracks.Items.Select(CriarDadosMusica).ToList();
    }
    
    private static DadosMusica CriarDadosMusica(FullTrack track)
    {
        var imagemUrl = track.Album.Images.FirstOrDefault()?.Url ?? string.Empty;
        track.ExternalUrls.TryGetValue("spotify", out var linkSpotify);
        
        return new DadosMusica
        {
            Id = track.Id,
            Nome = track.Name,
            Artista = track.Artists.FirstOrDefault()?.Name ?? string.Empty,
            ImagemUrl = imagemUrl,
            LinkSpotify = linkSpotify ?? string.Empty
        };
    }

    private async Task AtualizarTokenSeNecessarioAsync()
    {
        if (!TokenEstaValido())
        {
            lock (_bloqueioToken)
            {
                if (!TokenEstaValido())
                {
                    ObterTokenClientCredentialsAsync().GetAwaiter().GetResult();
                }
            }
        }
    }
    
    private async Task ObterTokenClientCredentialsAsync()
    {
        try
        {
            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient(config).RequestToken(
                new ClientCredentialsRequest(_clientId, _clientSecret));
            
            _tokenAtual = tokenResponse.AccessToken;
            _dataExpiracao = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            _spotifyClient = new SpotifyClient(tokenResponse.AccessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao obter token client credentials: {ex.Message}");
            throw;
        }
    }
}