using musicai.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("PermitirSpotify", politica =>
    {
        politica.WithOrigins(
                "https://accounts.spotify.com",
                "https://api.spotify.com",
                "http://localhost:3000",
                "http://localhost:5102",
                "http://127.0.0.1:5102")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed(_ => true)
               .AllowCredentials();
    });
    
    if (builder.Environment.IsDevelopment())
    {
        opcoes.AddPolicy("PermitirTudo", politica =>
        {
            politica.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
    }
});

builder.ConfigurarServicos();

var app = builder.Build();

// Aplica CORS antes de qualquer outro middleware
app.UseCors(app.Environment.IsDevelopment() ? "PermitirTudo" : "PermitirSpotify");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MusicAI API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Removida a linha de redirecionamento HTTPS
app.MapControllers();

app.Run();