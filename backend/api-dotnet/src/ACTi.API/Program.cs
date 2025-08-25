// backend/api-dotnet/src/ACTi.API/Program.cs
using Microsoft.EntityFrameworkCore;
using ACTi.Infrastructure.Data;
using ACTi.Infrastructure.Repositories;
using ACTi.Infrastructure.Services.ExternalApis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURAÇÃO DE SERVIÇOS
// ============================================================================

// Adicionar serviços básicos do ASP.NET Core
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar serialização JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = true; // Para desenvolvimento
    });

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ACTi API",
        Version = "v1",
        Description = "API para gerenciamento de parceiros comerciais - Sistema ACTi",
        Contact = new()
        {
            Name = "Equipe ACTi",
            Email = "dev@acti.com.br"
        }
    });

    // Incluir comentários XML na documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ⚡ CONFIGURAÇÃO DO BANCO DE DADOS
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    // Para desenvolvimento, usar SQLite se não houver connection string configurada
    connectionString = "Data Source=acti_development.db";
    builder.Services.AddDbContext<ACTiDbContext>(options =>
        options.UseSqlite(connectionString, b => b.MigrationsAssembly("ACTi.Infrastructure")));
}
else if (connectionString.Contains("PostgreSQL") || connectionString.Contains("postgres"))
{
    // Produção com PostgreSQL
    builder.Services.AddDbContext<ACTiDbContext>(options =>
        options.UseNpgsql(connectionString, b => b.MigrationsAssembly("ACTi.Infrastructure")));
}
else
{
    // SQL Server
    builder.Services.AddDbContext<ACTiDbContext>(options =>
        options.UseSqlServer(connectionString, b => b.MigrationsAssembly("ACTi.Infrastructure")));
}

// ⚡ CONFIGURAÇÃO MEDIATR (CQRS)
builder.Services.AddMediatR(cfg =>
{
    // Registrar handlers do assembly da Application layer
    cfg.RegisterServicesFromAssembly(typeof(ACTi.Application.Commands.CreatePartnerCommand).Assembly);
});

// ⚡ CONFIGURAÇÃO DOS REPOSITORIES
builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();

// ⚡ CONFIGURAÇÃO HTTP CLIENTS PARA APIS EXTERNAS
builder.Services.AddHttpClient<ViaCepService>(client =>
{
    client.BaseAddress = new Uri("https://viacep.com.br/ws/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "ACTi-API/1.0");
});

builder.Services.AddHttpClient<ReceitaWsService>(client =>
{
    client.BaseAddress = new Uri("https://receitaws.com.br/v1/cnpj/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "ACTi-API/1.0");
});

// ⚡ CONFIGURAÇÃO DOS SERVIÇOS EXTERNOS
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();

// ⚡ CONFIGURAÇÃO DE CORS (para desenvolvimento com Angular)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular dev server
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("ProductionCors", policy =>
    {
        // Em produção, especificar domínios exatos
        policy.WithOrigins("https://acti.com.br", "https://app.acti.com.br")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ⚡ CONFIGURAÇÃO DE LOGGING
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsProduction())
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// ⚡ CONFIGURAÇÃO DE HEALTHCHECKS
builder.Services.AddHealthChecks()
    .AddDbContext<ACTiDbContext>()
    .AddUrlGroup(new Uri("https://viacep.com.br"), "ViaCEP")
    .AddUrlGroup(new Uri("https://receitaws.com.br"), "ReceitaWS");

// ============================================================================
// CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÕES
// ============================================================================

var app = builder.Build();

// Configurar pipeline baseado no ambiente
if (app.Environment.IsDevelopment())
{
    // Desenvolvimento: Swagger e detalhes de erro
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ACTi API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz da API
        c.DisplayRequestDuration();
    });

    app.UseDeveloperExceptionPage();
    app.UseCors("DevelopmentCors");
}
else
{
    // Produção: Tratamento básico de exceções
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // HTTP Strict Transport Security
    app.UseCors("ProductionCors");
}

// Pipeline obrigatório
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Controllers
app.MapControllers();

// ⚡ INICIALIZAÇÃO DO BANCO DE DADOS
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ACTiDbContext>();

    try
    {
        // Aplicar migrações automaticamente
        await context.Database.MigrateAsync();

        app.Logger.LogInformation("Banco de dados inicializado com sucesso");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erro ao inicializar banco de dados");

        if (app.Environment.IsProduction())
        {
            throw; // Em produção, falhar se o banco não estiver disponível
        }
    }
}

// ============================================================================
// INICIALIZAÇÃO DA APLICAÇÃO
// ============================================================================

app.Logger.LogInformation("=== ACTi API Iniciada ===");
app.Logger.LogInformation("Ambiente: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("URLs: {Urls}", string.Join(", ", app.Urls));

if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("Swagger disponível em: /");
    app.Logger.LogInformation("Health checks: /health");
}

app.Run();