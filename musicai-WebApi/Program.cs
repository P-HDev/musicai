using musicai.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("PermitirTudo", politica =>
    {
        politica.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.ConfigurarServicos();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MusicAI API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("PermitirTudo");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();