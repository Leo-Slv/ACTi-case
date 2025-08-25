using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ACTi.Infrastructure.Services.ExternalApis
{
    /// <summary>
    /// Serviço para integração com API ReceitaWS
    /// https://receitaws.com.br/
    /// </summary>
    public class ReceitaWsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReceitaWsService> _logger;
        private const string BaseUrl = "https://receitaws.com.br/v1/cnpj/";

        public ReceitaWsService(HttpClient httpClient, ILogger<ReceitaWsService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configurar timeout maior para CNPJ (pode demorar mais)
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        /// <summary>
        /// Consulta CNPJ na API ReceitaWS
        /// </summary>
        /// <param name="cnpj">CNPJ para consulta (com ou sem formatação)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da empresa ou erro</returns>
        public async Task<CnpjResponse> ConsultarCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
        {
            try
            {
                // Limpar e validar CNPJ
                var cleanCnpj = LimparCnpj(cnpj);
                if (!ValidarCnpj(cleanCnpj))
                {
                    return new CnpjResponse
                    {
                        Success = false,
                        ErrorMessage = "CNPJ deve conter exatamente 14 dígitos"
                    };
                }

                _logger.LogInformation("Consultando CNPJ na ReceitaWS: {Cnpj}",
                    cleanCnpj.Substring(0, 8) + "****"); // Log parcial por segurança

                // Fazer requisição
                var url = $"{BaseUrl}{cleanCnpj}";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Erro na consulta ReceitaWS. Status: {StatusCode}", response.StatusCode);
                    return new CnpjResponse
                    {
                        Success = false,
                        ErrorMessage = "Erro na consulta do CNPJ"
                    };
                }

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var receitaResult = JsonSerializer.Deserialize<ReceitaWsResult>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (receitaResult == null)
                {
                    return new CnpjResponse
                    {
                        Success = false,
                        ErrorMessage = "Resposta inválida da API"
                    };
                }

                // Verificar se houve erro na consulta
                if (receitaResult.Status == "ERROR")
                {
                    _logger.LogInformation("CNPJ não encontrado ou inválido: {Message}", receitaResult.Message);
                    return new CnpjResponse
                    {
                        Success = false,
                        ErrorMessage = receitaResult.Message ?? "CNPJ não encontrado"
                    };
                }

                _logger.LogInformation("CNPJ consultado com sucesso: {RazaoSocial}", receitaResult.Nome);

                // Mapear para resposta padrão
                return new CnpjResponse
                {
                    Cnpj = FormatarCnpj(receitaResult.Cnpj),
                    RazaoSocial = receitaResult.Nome ?? string.Empty,
                    NomeFantasia = receitaResult.Fantasia ?? string.Empty,
                    Email = receitaResult.Email ?? string.Empty,
                    Telefone = receitaResult.Telefone ?? string.Empty,
                    Cep = LimparCep(receitaResult.Cep ?? string.Empty),
                    Logradouro = receitaResult.Logradouro ?? string.Empty,
                    Numero = receitaResult.Numero ?? string.Empty,
                    Complemento = receitaResult.Complemento ?? string.Empty,
                    Bairro = receitaResult.Bairro ?? string.Empty,
                    Municipio = receitaResult.Municipio ?? string.Empty,
                    Uf = receitaResult.Uf ?? string.Empty,
                    Situacao = receitaResult.Situacao ?? string.Empty,
                    Success = true
                };
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Consulta CNPJ cancelada");
                return new CnpjResponse
                {
                    Success = false,
                    ErrorMessage = "Consulta cancelada"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de rede na consulta CNPJ");
                return new CnpjResponse
                {
                    Success = false,
                    ErrorMessage = "Erro de conexão com o serviço de CNPJ"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na consulta CNPJ");
                return new CnpjResponse
                {
                    Success = false,
                    ErrorMessage = "Erro interno na consulta do CNPJ"
                };
            }
        }

        private static string LimparCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return string.Empty;

            return cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        }

        private static string LimparCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return string.Empty;

            return cep.Replace("-", "").Replace(".", "").Trim();
        }

        private static bool ValidarCnpj(string cnpj)
        {
            return !string.IsNullOrWhiteSpace(cnpj) &&
                   cnpj.Length == 14 &&
                   cnpj.All(char.IsDigit);
        }

        private static string FormatarCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj) || cnpj.Length != 14)
                return cnpj;

            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
        }

        /// <summary>
        /// Modelo para resposta da API ReceitaWS
        /// </summary>
        private class ReceitaWsResult
        {
            public string Status { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string Cnpj { get; set; } = string.Empty;
            public string Nome { get; set; } = string.Empty;
            public string Fantasia { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefone { get; set; } = string.Empty;
            public string Cep { get; set; } = string.Empty;
            public string Logradouro { get; set; } = string.Empty;
            public string Numero { get; set; } = string.Empty;
            public string Complemento { get; set; } = string.Empty;
            public string Bairro { get; set; } = string.Empty;
            public string Municipio { get; set; } = string.Empty;
            public string Uf { get; set; } = string.Empty;
            public string Situacao { get; set; } = string.Empty;
        }
    }
}