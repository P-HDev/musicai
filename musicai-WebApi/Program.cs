using musicai.Utils;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configurar HTTPS para desenvolvimento
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
    builder.WebHost.ConfigureKestrel(options =>
    {
        // Configurar para aceitar certificados de desenvolvimento
        options.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                                       System.Security.Authentication.SslProtocols.Tls13;
        });
    });
}

// Adicionar e configurar serviços
builder.ConfigurarServicos();

var app = builder.Build();

// Configurar o pipeline de request
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MusicAI API v1");
        c.RoutePrefix = string.Empty; // Para servir o Swagger UI na raiz
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Adicionar um redirecionamento para Swagger na raiz
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();