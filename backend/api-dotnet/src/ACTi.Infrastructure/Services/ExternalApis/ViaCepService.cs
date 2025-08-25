using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ACTi.Infrastructure.Services.ExternalApis
{
    /// <summary>
    /// Serviço para integração com API ViaCEP
    /// https://viacep.com.br/
    /// </summary>
    public class ViaCepService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ViaCepService> _logger;
        private const string BaseUrl = "https://viacep.com.br/ws/";

        public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configurar timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// Consulta CEP na API ViaCEP
        /// </summary>
        /// <param name="cep">CEP para consulta (com ou sem formatação)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados do endereço ou erro</returns>
        public async Task<CepResponse> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default)
        {
            try
            {
                // Limpar e validar CEP
                var cleanCep = LimparCep(cep);
                if (!ValidarCep(cleanCep))
                {
                    return new CepResponse
                    {
                        Success = false,
                        ErrorMessage = "CEP deve conter exatamente 8 dígitos"
                    };
                }

                _logger.LogInformation("Consultando CEP na ViaCEP: {Cep}", cleanCep);

                // Fazer requisição
                var url = $"{BaseUrl}{cleanCep}/json/";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Erro na consulta ViaCEP. Status: {StatusCode}", response.StatusCode);
                    return new CepResponse
                    {
                        Success = false,
                        ErrorMessage = "Erro na consulta do CEP"
                    };
                }

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var viaCepResult = JsonSerializer.Deserialize<ViaCepResult>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (viaCepResult == null)
                {
                    return new CepResponse
                    {
                        Success = false,
                        ErrorMessage = "Resposta inválida da API"
                    };
                }

                // Verificar se CEP existe
                if (viaCepResult.Erro)
                {
                    _logger.LogInformation("CEP não encontrado: {Cep}", cleanCep);
                    return new CepResponse
                    {
                        Success = false,
                        ErrorMessage = "CEP não encontrado"
                    };
                }

                _logger.LogInformation("CEP consultado com sucesso: {Cep} - {Localidade}/{Uf}",
                    cleanCep, viaCepResult.Localidade, viaCepResult.Uf);

                // Mapear para resposta padrão
                return new CepResponse
                {
                    Cep = FormatarCep(viaCepResult.Cep),
                    Logradouro = viaCepResult.Logradouro ?? string.Empty,
                    Complemento = viaCepResult.Complemento ?? string.Empty,
                    Bairro = viaCepResult.Bairro ?? string.Empty,
                    Localidade = viaCepResult.Localidade ?? string.Empty,
                    Uf = viaCepResult.Uf ?? string.Empty,
                    Success = true
                };
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Consulta CEP cancelada: {Cep}", cep);
                return new CepResponse
                {
                    Success = false,
                    ErrorMessage = "Consulta cancelada"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de rede na consulta CEP: {Cep}", cep);
                return new CepResponse
                {
                    Success = false,
                    ErrorMessage = "Erro de conexão com o serviço de CEP"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na consulta CEP: {Cep}", cep);
                return new CepResponse
                {
                    Success = false,
                    ErrorMessage = "Erro interno na consulta do CEP"
                };
            }
        }

        private static string LimparCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return string.Empty;

            return cep.Replace("-", "").Replace(".", "").Trim();
        }

        private static bool ValidarCep(string cep)
        {
            return !string.IsNullOrWhiteSpace(cep) &&
                   cep.Length == 8 &&
                   cep.All(char.IsDigit);
        }

        private static string FormatarCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep) || cep.Length != 8)
                return cep;

            return $"{cep.Substring(0, 5)}-{cep.Substring(5)}";
        }

        /// <summary>
        /// Modelo para resposta da API ViaCEP
        /// </summary>
        private class ViaCepResult
        {
            public string Cep { get; set; } = string.Empty;
            public string Logradouro { get; set; } = string.Empty;
            public string Complemento { get; set; } = string.Empty;
            public string Bairro { get; set; } = string.Empty;
            public string Localidade { get; set; } = string.Empty;
            public string Uf { get; set; } = string.Empty;
            public string Ibge { get; set; } = string.Empty;
            public string Gia { get; set; } = string.Empty;
            public string Ddd { get; set; } = string.Empty;
            public string Siafi { get; set; } = string.Empty;
            public bool Erro { get; set; }
        }
    }
}