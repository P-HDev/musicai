using Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI_API;

namespace Servico.Servicos;

public class OpenIAServico(IConfiguration configuracao) : IOpenIAServico
{
    private readonly OpenAIAPI _api = new(configuracao["OpenAISettings:ApiKey"]);
    private readonly string _modelo = configuracao["OpenAISettings:Model"] ?? "gpt-4.1-nano";

    public async Task<List<string>> GerarPlaylist(string mensagemUsuario, int numeroDeMusicas = 20)
    {
        if (string.IsNullOrEmpty(mensagemUsuario))
            return new List<string>();
            
        try
        {
            var conversa = CriarConversa(numeroDeMusicas);
            return await ProcessarSolicitacaoPlaylist(conversa, mensagemUsuario, numeroDeMusicas);
        }
        catch (Exception ex)
        {
            LogarErro(ex);
            return new List<string>();
        }
    }
    
    private OpenAI_API.Chat.Conversation CriarConversa(int numeroDeMusicas)
    {
        var conversa = _api.Chat.CreateConversation();
        conversa.Model = _modelo;
        conversa.AppendSystemMessage(
            $"Você é um assistente especializado em criar listas de músicas para playlists. " +
            $"Responda apenas com os nomes das músicas depois um espaço - nome do artista, " + 
            $"uma por linha, sem numeração, duração ou qualquer texto adicional. " +
            $"Limite-se a {numeroDeMusicas} músicas. Se não conseguir encontrar músicas relevantes, " +
            $"responda 'Nenhuma música encontrada.'");
            
        conversa.RequestParameters.Temperature = 0.5;
        conversa.RequestParameters.MaxTokens = numeroDeMusicas * 10;
        
        return conversa;
    }
    
    private static async Task<List<string>> ProcessarSolicitacaoPlaylist(
        OpenAI_API.Chat.Conversation conversa, 
        string mensagemUsuario,
        int numeroDeMusicas)
    {
        conversa.AppendUserInput($"Gere uma lista de {numeroDeMusicas} músicas para a seguinte solicitação: {mensagemUsuario}");
        
        string listaDeMusicas = await conversa.GetResponseFromChatbotAsync();
        
        if (listaDeMusicas.Equals("Nenhuma música encontrada.", StringComparison.OrdinalIgnoreCase))
            return new List<string>();

        return ExtrairNomesDeMusicas(listaDeMusicas);
    }
    
    private static List<string> ExtrairNomesDeMusicas(string listaDeMusicas) =>
        listaDeMusicas
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    
    private static void LogarErro(Exception ex) =>
        Console.WriteLine($"Erro ao gerar lista de músicas: {ex.Message}");
}