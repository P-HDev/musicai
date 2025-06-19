using Microsoft.Extensions.Configuration;
using OpenAI_API;

namespace Servico.Servicos;

public class OpenIA(IConfiguration configuration)
{
    private readonly OpenAIAPI _api = new OpenAIAPI(
        configuration["OpenAISettings:ApiKey"]);

    private readonly string _model =
        configuration["OpenAISettings:Model"] ?? "gpt-4.1-mini";

    public async Task<string> ProcessarMensagemAsync(string mensagemUsuario)
    {
        try
        {
            var chat = _api.Chat.CreateConversation();
            chat.Model = _model;

            chat.AppendUserInput(mensagemUsuario);

            string resposta = await chat.GetResponseFromChatbotAsync();
            return resposta;
        }
        catch (Exception ex)
        {
            return $"Erro ao processar mensagem: {ex.Message}";
        }
    }
}