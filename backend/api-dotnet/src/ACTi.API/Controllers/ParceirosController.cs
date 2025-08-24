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
        /// <returns>Dados do parceiro criado</returns>
        /// <response code="201">Parceiro criado com sucesso</response>
        /// <response code="400">Dados inválidos fornecidos</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(PartnerResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PartnerResponse>> CriarParceiro(
            [FromBody] CreatePartnerRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de parceiro: {CompanyName}", request.CompanyName);

                // Mapear Request para Command
                var command = MapToCommand(request);

                // Enviar command via MediatR
                var result = await _mediator.Send(command);

                _logger.LogInformation("Parceiro criado com sucesso. ID: {PartnerId}", result.Id);

                // Retornar 201 Created com dados do parceiro
                return CreatedAtAction(
                    nameof(ObterParceiro),
                    new { id = result.Id },
                    result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação ao criar parceiro: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar parceiro");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obter parceiro por ID (placeholder para CreatedAtAction)
        /// </summary>
        /// <param name="id">ID do parceiro</param>
        /// <returns>Dados do parceiro</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PartnerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartnerResponse>> ObterParceiro(int id)
        {
            // TODO: Implementar quando tivermos Query/Repository
            _logger.LogInformation("Tentativa de obter parceiro ID: {PartnerId}", id);

            return NotFound(new { error = "Funcionalidade ainda não implementada" });
        }

        /// <summary>
        /// Endpoint de teste para verificar se API está funcionando
        /// </summary>
        /// <returns>Mensagem de teste</returns>
        [HttpGet("test")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult Test()
        {
            _logger.LogInformation("Endpoint de teste chamado");

            return Ok(new
            {
                message = "ACTi API está funcionando!",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
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
    }
}