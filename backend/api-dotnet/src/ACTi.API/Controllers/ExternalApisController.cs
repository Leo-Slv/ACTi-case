// backend/api-dotnet/src/ACTi.API/Controllers/ExternalApisController.cs
using Microsoft.AspNetCore.Mvc;
using ACTi.Infrastructure.Services.ExternalApis;

namespace ACTi.API.Controllers
{
    /// <summary>
    /// Controller para integração com APIs externas
    /// Expõe endpoints para consulta de CEP e CNPJ
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ExternalApisController : ControllerBase
    {
        private readonly IExternalApiService _externalApiService;
        private readonly ILogger<ExternalApisController> _logger;

        public ExternalApisController(
            IExternalApiService externalApiService,
            ILogger<ExternalApisController> logger)
        {
            _externalApiService = externalApiService;
            _logger = logger;
        }

        /// <summary>
        /// Consultar dados de endereço por CEP
        /// </summary>
        /// <param name="cep">CEP para consulta (com ou sem formatação)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados do endereço</returns>
        /// <response code="200">Dados do endereço encontrados</response>
        /// <response code="400">CEP inválido</response>
        /// <response code="404">CEP não encontrado</response>
        /// <response code="500">Erro interno do servidor</response>
        /// <example>
        /// GET /api/externalaapis/cep/12345678
        /// GET /api/externalaapis/cep/12345-678
        /// </example>
        [HttpGet("cep/{cep}")]
        [ProducesResponseType(typeof(CepResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CepResponse>> ConsultarCep(
            [FromRoute] string cep,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Recebida consulta de CEP: {Cep}", cep?.Substring(0, 5) + "***");

                if (string.IsNullOrWhiteSpace(cep))
                {
                    return BadRequest(new { error = "CEP é obrigatório" });
                }

                var result = await _externalApiService.ConsultarCepAsync(cep, cancellationToken);

                if (!result.Success)
                {
                    if (result.ErrorMessage.Contains("não encontrado"))
                    {
                        return NotFound(new { error = result.ErrorMessage });
                    }

                    return BadRequest(new { error = result.ErrorMessage });
                }

                _logger.LogInformation("CEP consultado com sucesso: {Localidade}/{Uf}",
                    result.Localidade, result.Uf);

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Consulta de CEP cancelada");
                return BadRequest(new { error = "Consulta cancelada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na consulta de CEP");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Consultar dados de empresa por CNPJ
        /// </summary>
        /// <param name="cnpj">CNPJ para consulta (com ou sem formatação)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da empresa</returns>
        /// <response code="200">Dados da empresa encontrados</response>
        /// <response code="400">CNPJ inválido</response>
        /// <response code="404">CNPJ não encontrado</response>
        /// <response code="500">Erro interno do servidor</response>
        /// <example>
        /// GET /api/externalaapis/cnpj/12345678000195
        /// GET /api/externalaapis/cnpj/12.345.678/0001-95
        /// </example>
        [HttpGet("cnpj/{cnpj}")]
        [ProducesResponseType(typeof(CnpjResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CnpjResponse>> ConsultarCnpj(
            [FromRoute] string cnpj,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Recebida consulta de CNPJ");

                if (string.IsNullOrWhiteSpace(cnpj))
                {
                    return BadRequest(new { error = "CNPJ é obrigatório" });
                }

                var result = await _externalApiService.ConsultarCnpjAsync(cnpj, cancellationToken);

                if (!result.Success)
                {
                    if (result.ErrorMessage.Contains("não encontrado") ||
                        result.ErrorMessage.Contains("inválido"))
                    {
                        return NotFound(new { error = result.ErrorMessage });
                    }

                    return BadRequest(new { error = result.ErrorMessage });
                }

                _logger.LogInformation("CNPJ consultado com sucesso: {RazaoSocial}", result.RazaoSocial);

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Consulta de CNPJ cancelada");
                return BadRequest(new { error = "Consulta cancelada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na consulta de CNPJ");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Consultar CEP e CNPJ em uma única requisição (para formulário)
        /// </summary>
        /// <param name="request">Dados para consulta</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados combinados de CEP e CNPJ</returns>
        /// <response code="200">Consultas realizadas</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost("consulta-completa")]
        [ProducesResponseType(typeof(ConsultaCompletaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConsultaCompletaResponse>> ConsultaCompleta(
            [FromBody] ConsultaCompletaRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Iniciando consulta completa");

                var response = new ConsultaCompletaResponse();

                // Lista de tarefas para executar em paralelo
                var tasks = new List<Task>();

                // Consultar CEP se fornecido
                if (!string.IsNullOrWhiteSpace(request.Cep))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        response.CepData = await _externalApiService.ConsultarCepAsync(request.Cep, cancellationToken);
                    }, cancellationToken));
                }

                // Consultar CNPJ se fornecido
                if (!string.IsNullOrWhiteSpace(request.Cnpj))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        response.CnpjData = await _externalApiService.ConsultarCnpjAsync(request.Cnpj, cancellationToken);
                    }, cancellationToken));
                }

                if (tasks.Count == 0)
                {
                    return BadRequest(new { error = "Pelo menos CEP ou CNPJ deve ser fornecido" });
                }

                // Aguardar todas as consultas
                await Task.WhenAll(tasks);

                _logger.LogInformation("Consulta completa finalizada");

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Consulta completa cancelada");
                return BadRequest(new { error = "Consulta cancelada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na consulta completa");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Verificar status dos serviços externos
        /// </summary>
        /// <returns>Status dos serviços</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> VerificarStatus()
        {
            var status = new
            {
                timestamp = DateTime.UtcNow,
                services = new
                {
                    viaCep = new
                    {
                        name = "ViaCEP",
                        url = "https://viacep.com.br",
                        status = "OK", // Em implementação real, fazer health check
                        description = "Consulta de CEP"
                    },
                    receitaWs = new
                    {
                        name = "ReceitaWS",
                        url = "https://receitaws.com.br",
                        status = "OK", // Em implementação real, fazer health check
                        description = "Consulta de CNPJ"
                    }
                }
            };

            return Ok(status);
        }
    }

    /// <summary>
    /// DTO para requisição de consulta completa
    /// </summary>
    public class ConsultaCompletaRequest
    {
        /// <summary>
        /// CEP para consulta (opcional)
        /// </summary>
        public string? Cep { get; set; }

        /// <summary>
        /// CNPJ para consulta (opcional)
        /// </summary>
        public string? Cnpj { get; set; }
    }

    /// <summary>
    /// DTO para resposta de consulta completa
    /// </summary>
    public class ConsultaCompletaResponse
    {
        /// <summary>
        /// Dados do CEP (se consultado)
        /// </summary>
        public CepResponse? CepData { get; set; }

        /// <summary>
        /// Dados do CNPJ (se consultado)
        /// </summary>
        public CnpjResponse? CnpjData { get; set; }
    }
}