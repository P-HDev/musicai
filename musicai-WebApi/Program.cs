using musicai.Utils;

var builder = WebApplication.CreateBuilder(args);

// Adicionar e configurar serviços
builder.ConfigurarServicos();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var app = builder.Build();

// Configurar o pipeline de request
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();