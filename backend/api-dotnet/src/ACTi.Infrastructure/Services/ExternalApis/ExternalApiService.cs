using Microsoft.Extensions.Logging;

namespace ACTi.Infrastructure.Services.ExternalApis
{
    /// <summary>
    /// Serviço agregador para APIs externas
    /// Unifica chamadas para ViaCEP e ReceitaWS
    /// </summary>
    public class ExternalApiService : IExternalApiService
    {
        private readonly ViaCepService _viaCepService;
        private readonly ReceitaWsService _receitaWsService;
        private readonly ILogger<ExternalApiService> _logger;

        public ExternalApiService(
            ViaCepService viaCepService,
            ReceitaWsService receitaWsService,
            ILogger<ExternalApiService> logger)
        {
            _viaCepService = viaCepService ?? throw new ArgumentNullException(nameof(viaCepService));
            _receitaWsService = receitaWsService ?? throw new ArgumentNullException(nameof(receitaWsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Consulta dados de endereço por CEP via ViaCEP
        /// </summary>
        /// <param name="cep">CEP para consulta</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados do endereço ou erro</returns>
        public async Task<CepResponse> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando consulta de CEP: {Cep}", cep?.Substring(0, Math.Min(5, cep.Length)) + "***");

            try
            {
                var result = await _viaCepService.ConsultarCepAsync(cep, cancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation("Consulta CEP realizada com sucesso");
                }
                else
                {
                    _logger.LogWarning("Falha na consulta CEP: {Error}", result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na consulta de CEP");
                return new CepResponse
                {
                    Success = false,
                    ErrorMessage = "Erro interno na consulta do CEP"
                };
            }
        }

        /// <summary>
        /// Consulta dados de empresa por CNPJ via ReceitaWS
        /// </summary>
        /// <param name="cnpj">CNPJ para consulta</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da empresa ou erro</returns>
        public async Task<CnpjResponse> ConsultarCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando consulta de CNPJ");

            try
            {
                var result = await _receitaWsService.ConsultarCnpjAsync(cnpj, cancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation("Consulta CNPJ realizada com sucesso: {RazaoSocial}", result.RazaoSocial);
                }
                else
                {
                    _logger.LogWarning("Falha na consulta CNPJ: {Error}", result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na consulta de CNPJ");
                return new CnpjResponse
                {
                    Success = false,
                    ErrorMessage = "Erro interno na consulta do CNPJ"
                };
            }
        }
    }
}