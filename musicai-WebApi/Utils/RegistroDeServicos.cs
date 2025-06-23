using Servico.Servicos;
using System.Net;
using Microsoft.EntityFrameworkCore;
using musicai.Data;

namespace musicai.Utils;

public static class RegistroDeServicos
{
    public static WebApplicationBuilder ConfigurarServicos(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.ConfigureSwagger();

        // Configuração do contexto do banco de dados PostgreSQL
        builder.Services.AddDbContext<MusicAIDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();

            builder.Services.AddHttpClient("HttpsClient")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                });
            
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                                                System.Security.Authentication.SslProtocols.Tls13;
                });
            });
        }

        builder.Services.ConfigureHttpsRedirection(builder.Environment);
        
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
                    Email = "anajuliapixaoSANTOS@hotmail.com"
                }
            });
            
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
                options.HttpsPort = 7185;
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