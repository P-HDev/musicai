using Servico.Servicos;

namespace musicai.Utils;

public static class RegistroDeServicos
{
    public static WebApplicationBuilder ConfigurarServicos(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.ConfigureSwagger();

        builder.Services.AddScoped<OpenIA>();

        return builder;
    }

    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "MusicAI API",
                Version = "v1",
                Description = "API para montar sua playlist de músicas com base em mensagens de texto.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Suporte MusicAI",
                    Email = "pnajuliapixaoSANTOS@hotmil.com"
                }
            });
        });

        return services;
    }
}