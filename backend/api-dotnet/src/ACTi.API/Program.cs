var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao container
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ⚡ CONFIGURAÇÃO MEDIATR - CORRIGIDA para encontrar Handlers
builder.Services.AddMediatR(cfg =>
{
    // Registrar handlers do assembly da Application layer
    cfg.RegisterServicesFromAssembly(typeof(ACTi.Application.Commands.CreatePartnerCommand).Assembly);
});

var app = builder.Build();

// Configurar pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();