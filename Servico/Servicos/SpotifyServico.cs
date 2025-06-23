using Dominio.Entidades;
using Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;

namespace Servico.Servicos;

public class SpotifyServico : ISpotifyServico
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;
    private ISpotifyClient? _spotifyClient;
    private string? _tokenAtual;
    private DateTime _dataExpiracao = DateTime.MinValue;
    private readonly object _bloqueioToken = new();
    private static readonly string[] _escopos = { 
        "user-read-private", 
        "user-read-email", 
        "playlist-modify-public", 
        "playlist-modify-private",
        "playlist-read-private",
        "playlist-read-collaborative"
    };

    public SpotifyServico(IConfiguration configuracao)
    {
        _clientId = configuracao["SpotifyConfiguracao:ClientId"] ?? 
                throw new ArgumentNullException(nameof(configuracao), "ClientId não configurado");
        _clientSecret = configuracao["SpotifyConfiguracao:ClientSecret"] ?? 
                    throw new ArgumentNullException(nameof(configuracao), "ClientSecret não configurado");
        _redirectUri = configuracao["SpotifyConfiguracao:RedirectUri"] ?? 
                    throw new ArgumentNullException(nameof(configuracao), "RedirectUri não configurado");
        
        InicializarTokenSincrono();
    }

    public SpotifyServico() : this(null!)
    {
        // Construtor sem parâmetros para compatibilidade com DI se necessário
    }

    public string ObterUrlAutorizacao()
    {
        var loginRequest = new LoginRequest(
            new Uri(_redirectUri),
            _clientId,
            LoginRequest.ResponseType.Code)
        {
            Scope = _escopos
        };

        return loginRequest.ToUri().ToString();
    }

    public async Task<AutenticacaoSpotify> ObterTokenUsuarioAsync(string codigo)
    {
        if (string.IsNullOrEmpty(codigo))
            throw new ArgumentException("Código de autorização não pode ser vazio", nameof(codigo));

        try
        {
            var response = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(
                    _clientId,
                    _clientSecret,
                    codigo,
                    new Uri(_redirectUri)
                )
            );

            return new AutenticacaoSpotify
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                TokenType = response.TokenType,
                ExpiresIn = response.ExpiresIn,
                DataExpiracao = DateTime.UtcNow.AddSeconds(response.ExpiresIn),
                Scope = response.Scope
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao obter token de acesso do usuário", ex);
        }
    }

    public async Task<AutenticacaoSpotify> AtualizarTokenUsuarioAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("Token de atualização não pode ser vazio", nameof(refreshToken));

        try
        {
            var response = await new OAuthClient().RequestToken(
                new AuthorizationCodeRefreshRequest(_clientId, _clientSecret, refreshToken)
            );

            return new AutenticacaoSpotify
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken ?? refreshToken, // Mantém o refresh token anterior se não for fornecido
                TokenType = response.TokenType,
                ExpiresIn = response.ExpiresIn,
                DataExpiracao = DateTime.UtcNow.AddSeconds(response.ExpiresIn),
                Scope = response.Scope
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao atualizar token de acesso do usuário", ex);
        }
    }

    public async Task<bool> CriarPlaylistUsuarioAsync(string nomePlaylist, string descricao, List<string> trackIds, string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentException("Token de acesso não pode ser vazio", nameof(accessToken));

        if (string.IsNullOrEmpty(nomePlaylist))
            throw new ArgumentException("Nome da playlist não pode ser vazio", nameof(nomePlaylist));

        if (trackIds == null || !trackIds.Any())
            throw new ArgumentException("A lista de músicas não pode estar vazia", nameof(trackIds));

        try
        {
            var spotifyClient = new SpotifyClient(accessToken);
            var usuarioAtual = await spotifyClient.UserProfile.Current();
            
            Console.WriteLine($"Criando playlist para o usuário: {usuarioAtual.Id}");
            
            var playlistCriada = await spotifyClient.Playlists.Create(usuarioAtual.Id, new PlaylistCreateRequest(nomePlaylist)
            {
                Description = descricao,
                Public = false
            });
            
            Console.WriteLine($"Playlist criada com ID: {playlistCriada.Id}");

            // Removendo possíveis IDs duplicados e inválidos
            var trackIdsUnicos = trackIds.Distinct().Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            Console.WriteLine($"Adicionando {trackIdsUnicos.Count} faixas únicas à playlist");
            
            // Convertendo os IDs das faixas em URIs no formato esperado pelo Spotify
            var urisComoStrings = trackIdsUnicos.Select(id => $"spotify:track:{id}").ToList();
            
            // Adicionando faixas em lotes para evitar exceder limites da API
            const int tamanhoBatch = 50;
            for (int i = 0; i < urisComoStrings.Count; i += tamanhoBatch)
            {
                var batch = urisComoStrings.Skip(i).Take(tamanhoBatch).ToList();
                await spotifyClient.Playlists.AddItems(playlistCriada.Id, new PlaylistAddItemsRequest(batch));
                Console.WriteLine($"Adicionado lote de {batch.Count} faixas");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            // Capturando detalhes mais específicos da exceção
            Console.WriteLine($"Erro ao criar playlist: {ex.GetType().Name}: {ex.Message}");
            
            if (ex.InnerException != null)
                Console.WriteLine($"Exceção interna: {ex.InnerException.Message}");
                
            // Se for uma exceção da API do Spotify, pode ter mais detalhes
            if (ex is APIException apiEx)
                Console.WriteLine($"Erro da API Spotify: Status: {apiEx.Response?.StatusCode}, Body: {apiEx.Response?.Body}");
                
            throw new InvalidOperationException($"Falha ao criar playlist no Spotify: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<PlaylistSpotify>> ObterPlaylistsUsuarioAsync(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentNullException(nameof(accessToken), "Token de acesso não pode ser nulo ou vazio");

        try
        {
            var clientConfig = SpotifyClientConfig.CreateDefault().WithToken(accessToken);
            var spotifyClient = new SpotifyClient(clientConfig);
            
            var playlistsResponse = await spotifyClient.Playlists.CurrentUsers();
            var playlists = new List<PlaylistSpotify>();
            
            foreach (var item in playlistsResponse.Items)
            {
                var playlist = new PlaylistSpotify
                {
                    Id = item.Id,
                    Nome = item.Name,
                    Descricao = item.Description ?? string.Empty,
                    TotalFaixas = Convert.ToInt32(item.Tracks.Total),
                    Publica = item.Public ?? false,
                    Colaborativa = Convert.ToBoolean(item.Collaborative),
                    UrlExterna = item.ExternalUrls.ContainsKey("spotify") 
                        ? item.ExternalUrls["spotify"] 
                        : string.Empty
                };
                
                if (item.Images != null && item.Images.Count > 0)
                {
                    playlist.ImagemUrl = item.Images[0].Url;
                }
                
                playlists.Add(playlist);
            }
            
            return playlists;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao obter playlists do usuário: {ex.Message}", ex);
        }
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

    public string ObterClientId()
    {
        return _clientId;
    }
}