using Microsoft.Extensions.Configuration;
using OpenAI_API;

namespace Servico.Servicos;

public class OpenIA(IConfiguration configuration)
{
    private readonly OpenAIAPI _api = new OpenAIAPI(
        configuration["OpenAISettings:ApiKey"]);

    private readonly string _model =
        configuration["OpenAISettings:Model"];

    public async Task<List<string>> GerarPlaylist(string mensagemUsuario)
    {
        try
        {
            var chat = _api.Chat.CreateConversation();
            chat.Model = _model;
            chat.AppendSystemMessage(
                "Você é um assistente especializado em criar listas de músicas para playlists. Responda apenas com os nomes das músicas, uma por linha, sem numeração, artistas, duração ou qualquer texto adicional. Limite-se a 20 músicas. Se não conseguir encontrar músicas relevantes, responda 'Nenhuma música encontrada.'");

            chat.AppendUserInput($"Gere uma lista de 20 músicas para a seguinte solicitação: {mensagemUsuario}");

            chat.RequestParameters.Temperature = 0.4;
            chat.RequestParameters.MaxTokens = 200;

            string listaDeMusicas = await chat.GetResponseFromChatbotAsync();

            if (listaDeMusicas.Equals("Nenhuma música encontrada.", StringComparison.OrdinalIgnoreCase))
                return new List<string>();

            List<string> musicas = listaDeMusicas
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            return musicas;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao gerar lista de músicas: {ex.Message}");
            return new List<string>();
        }
    }
}