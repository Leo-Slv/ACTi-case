using Microsoft.AspNetCore.Mvc;
using MediatR;
using ACTi.Application.Commands;
using ACTi.Application.DTOs.Requests;
using ACTi.Application.DTOs.Responses;

namespace ACTi.API.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de parceiros comerciais
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ParceirosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ParceirosController> _logger;

        public ParceirosController(IMediator mediator, ILogger<ParceirosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Criar novo parceiro comercial
        /// </summary>
        /// <param name="request">Dados do parceiro a ser criado</param>
        /// <returns>Resposta padronizada com dados do parceiro criado</returns>
        /// <response code="201">Parceiro criado com sucesso</response>
        /// <response code="400">Dados inválidos ou parceiro já existe</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(StandardApiResponse<PartnerResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StandardApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StandardApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<StandardApiResponse<PartnerResponse>>> CriarParceiro(
            [FromBody] CreatePartnerRequest request)
        {
            try
            {
                // Validação se request é null
                if (request == null)
                {
                    _logger.LogWarning("Request nulo recebido");
                    return BadRequest(CreateErrorResponse<object>(
                        "Os dados do parceiro são obrigatórios",
                        "NULL_REQUEST"
                    ));
                }

                _logger.LogInformation("Iniciando criação de parceiro: {CompanyName}", request.CompanyName ?? "Nome não informado");

                // Validação básica do modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    var firstError = errors.FirstOrDefault();
                    var errorMessage = firstError.Value?.FirstOrDefault() ?? "Dados inválidos fornecidos";

                    return BadRequest(CreateErrorResponse<object>(
                        errorMessage,
                        "VALIDATION_ERROR",
                        errors
                    ));
                }

                // Mapear Request para Command
                var command = MapToCommand(request);

                // Enviar command via MediatR
                var result = await _mediator.Send(command);

                _logger.LogInformation("Parceiro criado com sucesso. ID: {PartnerId}", result.Id);

                // Retornar 201 Created com resposta padronizada
                return CreatedAtAction(
                    nameof(CriarParceiro),
                    new { id = result.Id },
                    CreateSuccessResponse(result, "Parceiro cadastrado com sucesso")
                );
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning("Argumento nulo ao criar parceiro: {Error}", ex.Message);

                return BadRequest(CreateErrorResponse<object>(
                    "Campo obrigatório não informado",
                    "REQUIRED_FIELD_ERROR"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação ao criar parceiro: {Error}", ex.Message);

                var message = string.IsNullOrEmpty(ex.Message) ? "Dados inválidos fornecidos" : ex.Message;

                return BadRequest(CreateErrorResponse<object>(
                    message,
                    "VALIDATION_ERROR"
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Erro de regra de negócio ao criar parceiro: {Error}", ex.Message);

                var message = string.IsNullOrEmpty(ex.Message) ? "Operação não permitida" : ex.Message;

                return BadRequest(CreateErrorResponse<object>(
                    message,
                    "BUSINESS_RULE_ERROR"
                ));
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning("Timeout ao criar parceiro: {Error}", ex.Message);

                return StatusCode(408, CreateErrorResponse<object>(
                    "Operação demorou muito para ser processada. Tente novamente.",
                    "TIMEOUT_ERROR"
                ));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Acesso negado ao criar parceiro: {Error}", ex.Message);

                return StatusCode(403, CreateErrorResponse<object>(
                    "Acesso negado para esta operação",
                    "ACCESS_DENIED"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno inesperado ao criar parceiro");

                return StatusCode(500, CreateErrorResponse<object>(
                    "Erro interno do servidor. Tente novamente mais tarde.",
                    "INTERNAL_ERROR",
                    new
                    {
                        errorType = ex.GetType().Name,
                        timestamp = DateTime.UtcNow
                    }
                ));
            }
        }

        /// <summary>
        /// Mapear CreatePartnerRequest para CreatePartnerCommand
        /// </summary>
        private static CreatePartnerCommand MapToCommand(CreatePartnerRequest request)
        {
            return new CreatePartnerCommand
            {
                PersonalityType = request.PersonalityType,
                CompanyName = request.CompanyName,
                Document = request.Document,
                ZipCode = request.ZipCode,
                State = request.State,
                City = request.City,
                Street = request.Street,
                Number = request.Number,
                Neighborhood = request.Neighborhood,
                Email = request.Email,
                Phone = request.Phone,
                Complement = request.Complement,
                Observation = request.Observation
            };
        }

        /// <summary>
        /// Criar resposta de sucesso padronizada
        /// </summary>
        private static StandardApiResponse<T> CreateSuccessResponse<T>(T data, string message = "Operação realizada com sucesso")
        {
            return new StandardApiResponse<T>
            {
                Success = true,
                Message = message,
                Code = "SUCCESS",
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Criar resposta de erro padronizada
        /// </summary>
        private static StandardApiResponse<T> CreateErrorResponse<T>(string message, string code, object? details = null)
        {
            return new StandardApiResponse<T>
            {
                Success = false,
                Message = message,
                Code = code,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Resposta padronizada para API ACTi
    /// </summary>
    public class StandardApiResponse<T>
    {
        /// <summary>
        /// Indica se a operação foi bem-sucedida
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem legível para o usuário
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Código identificador do tipo de erro/sucesso
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Dados retornados pela operação
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Detalhes adicionais (erros de validação, etc)
        /// </summary>
        public object? Details { get; set; }

        /// <summary>
        /// Timestamp da resposta
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}