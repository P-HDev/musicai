using Servico.Servicos;
using System.Net;

namespace musicai.Utils;

public static class RegistroDeServicos
{
    public static WebApplicationBuilder ConfigurarServicos(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.ConfigureSwagger();

        // Configurar HttpClient para ignorar erros de certificado em desenvolvimento
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpClient("HttpsClient")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                });
        }

        // Configurar redirecionamento HTTPS
        builder.Services.ConfigureHttpsRedirection(builder.Environment);

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
                    Email = "pnajuliapixaoSANTOS@hotmail.com"
                }
            });
            
            // Garantir que o Swagger funcione corretamente em ambiente de desenvolvimento
            c.CustomSchemaIds(type => type.FullName);
        });

        return services;
    }

    public static IServiceCollection ConfigureHttpsRedirection(this IServiceCollection services, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
                options.HttpsPort = 7185; // De acordo com launchSettings.json
            });
        }
        else
        {
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                options.HttpsPort = 443;
            });
        }

        return services;
    }
}