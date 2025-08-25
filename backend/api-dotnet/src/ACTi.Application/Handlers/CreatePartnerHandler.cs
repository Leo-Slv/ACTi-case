using System;
using System.Threading;
using System.Threading.Tasks;
using ACTi.Application.Commands;
using ACTi.Application.DTOs.Responses;
using ACTi.Application.Repositories;
using ACTi.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ACTi.Application.Handlers
{
    /// <summary>
    /// Handler responsável por processar o comando de criação de parceiro
    /// Implementa a lógica de negócio, validações e orquestração
    /// </summary>
    public class CreatePartnerHandler : IRequestHandler<CreatePartnerCommand, PartnerResponse>
    {
        private readonly IPartnerRepository _partnerRepository; 
        private readonly ILogger<CreatePartnerHandler> _logger;

        public CreatePartnerHandler(
            IPartnerRepository partnerRepository,
            ILogger<CreatePartnerHandler> logger)
        {
            _partnerRepository = partnerRepository ?? throw new ArgumentNullException(nameof(partnerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processa o comando de criar parceiro
        /// </summary>
        /// <param name="request">Comando com dados do parceiro</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta com dados do parceiro criado</returns>
        public async Task<PartnerResponse> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando criação de parceiro: {CompanyName}, Tipo: {PersonalityType}",
                request.CompanyName, request.PersonalityType);

            try
            {
                // 1. Validar entrada
                await ValidateRequestAsync(request, cancellationToken);

                // 2. Criar entidade Partner
                var partner = CreatePartnerEntity(request);

                // 3. Validar duplicidade
                await ValidateDuplicatesAsync(partner, cancellationToken);

                // 4. Salvar no banco via Repository
                var savedPartner = await _partnerRepository.AddAsync(partner, cancellationToken);

                _logger.LogInformation("Parceiro criado com sucesso. ID: {PartnerId}, Nome: {CompanyName}",
                    savedPartner.Id, savedPartner.CompanyName);

                // 5. Mapear para DTO de resposta
                var response = MapToResponse(savedPartner);

                return response;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação ao criar parceiro: {Error}", ex.Message);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Erro de regra de negócio ao criar parceiro: {Error}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar parceiro: {CompanyName}", request.CompanyName);
                throw new InvalidOperationException("Erro interno ao processar criação do parceiro", ex);
            }
        }

        /// <summary>
        /// Valida os dados da requisição
        /// </summary>
        private async Task ValidateRequestAsync(CreatePartnerCommand request, CancellationToken cancellationToken)
        {
            // Validações básicas de entrada
            if (string.IsNullOrWhiteSpace(request.PersonalityType))
                throw new ArgumentException("Tipo de personalidade é obrigatório");

            if (request.PersonalityType != "F" && request.PersonalityType != "J")
                throw new ArgumentException("Tipo de personalidade deve ser 'F' (Física) ou 'J' (Jurídica)");

            if (string.IsNullOrWhiteSpace(request.CompanyName))
                throw new ArgumentException("Nome/Razão social é obrigatório");

            if (string.IsNullOrWhiteSpace(request.Document))
                throw new ArgumentException("CNPJ/CPF é obrigatório");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email é obrigatório");

            // Validações específicas por tipo
            if (request.PersonalityType == "J")
            {
                ValidateLegalPersonData(request);
            }
            else
            {
                ValidateNaturalPersonData(request);
            }

            _logger.LogDebug("Validações básicas concluídas para {PersonalityType}", request.PersonalityType);
        }

        /// <summary>
        /// Validações específicas para pessoa jurídica
        /// </summary>
        private static void ValidateLegalPersonData(CreatePartnerCommand request)
        {
            // Validar comprimento do CNPJ (básico, o Value Object fará validação completa)
            var cleanCnpj = request.Document.Replace(".", "").Replace("/", "").Replace("-", "");
            if (cleanCnpj.Length != 14)
                throw new ArgumentException("CNPJ deve ter 14 dígitos");

            if (!cleanCnpj.All(char.IsDigit))
                throw new ArgumentException("CNPJ deve conter apenas números");
        }

        /// <summary>
        /// Validações específicas para pessoa física
        /// </summary>
        private static void ValidateNaturalPersonData(CreatePartnerCommand request)
        {
            // Validar comprimento do CPF (básico, o Value Object fará validação completa)
            var cleanCpf = request.Document.Replace(".", "").Replace("-", "");
            if (cleanCpf.Length != 11)
                throw new ArgumentException("CPF deve ter 11 dígitos");

            if (!cleanCpf.All(char.IsDigit))
                throw new ArgumentException("CPF deve conter apenas números");
        }

        /// <summary>
        /// Cria a entidade Partner baseada no tipo de personalidade
        /// </summary>
        private Partner CreatePartnerEntity(CreatePartnerCommand request)
        {
            try
            {
                if (request.PersonalityType == "J")
                {
                    _logger.LogDebug("Criando pessoa jurídica com CNPJ: {Document}",
                        request.Document.Substring(0, 8) + "****"); // Log parcial por segurança

                    return Partner.CreateLegalPerson(
                        companyName: request.CompanyName,
                        cnpj: request.Document,
                        email: request.Email,
                        zipCode: request.ZipCode,
                        state: request.State,
                        city: request.City,
                        street: request.Street,
                        number: request.Number,
                        neighborhood: request.Neighborhood,
                        phone: request.Phone,
                        complement: request.Complement,
                        observations: request.Observation
                    );
                }
                else // PersonalityType == "F"
                {
                    _logger.LogDebug("Criando pessoa física com CPF: {Document}",
                        request.Document.Substring(0, 6) + "*****"); // Log parcial por segurança

                    return Partner.CreateNaturalPerson(
                        fullName: request.CompanyName,
                        cpf: request.Document,
                        email: request.Email,
                        zipCode: request.ZipCode,
                        state: request.State,
                        city: request.City,
                        street: request.Street,
                        number: request.Number,
                        neighborhood: request.Neighborhood,
                        phone: request.Phone,
                        complement: request.Complement,
                        observations: request.Observation
                    );
                }
            }
            catch (ArgumentException ex)
            {
                // Re-throw com contexto mais claro
                throw new ArgumentException($"Erro na validação dos dados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Valida se já existe parceiro com os mesmos dados únicos
        /// </summary>
        private async Task ValidateDuplicatesAsync(Partner partner, CancellationToken cancellationToken)
        {
            // Verificar duplicidade de CNPJ (para pessoa jurídica)
            if (partner.Cnpj != null)
            {
                var cnpjExists = await _partnerRepository.CnpjExistsAsync(partner.Cnpj.Number, cancellationToken: cancellationToken);
                if (cnpjExists)
                {
                    _logger.LogWarning("Tentativa de cadastro com CNPJ já existente: {CNPJ}",
                        partner.Cnpj.Formatted.Substring(0, 8) + "****");
                    throw new InvalidOperationException("Já existe um parceiro cadastrado com este CNPJ");
                }
            }

            // Verificar duplicidade de CPF (para pessoa física)
            if (partner.Cpf != null)
            {
                var cpfExists = await _partnerRepository.CpfExistsAsync(partner.Cpf.Number, cancellationToken: cancellationToken);
                if (cpfExists)
                {
                    _logger.LogWarning("Tentativa de cadastro com CPF já existente: {CPF}",
                        partner.Cpf.Formatted.Substring(0, 6) + "*****");
                    throw new InvalidOperationException("Já existe um parceiro cadastrado com este CPF");
                }
            }

            // Verificar duplicidade de email
            var emailExists = await _partnerRepository.EmailExistsAsync(partner.Email.Address, cancellationToken: cancellationToken);
            if (emailExists)
            {
                _logger.LogWarning("Tentativa de cadastro com email já existente: {Email}", partner.Email.Address);
                throw new InvalidOperationException("Já existe um parceiro cadastrado com este email");
            }

            _logger.LogDebug("Validação de duplicidade concluída - nenhum conflito encontrado");
        }

        /// <summary>
        /// Mapeia entidade Partner para DTO de resposta
        /// </summary>
        private static PartnerResponse MapToResponse(Partner partner)
        {
            return new PartnerResponse
            {
                Id = partner.Id,
                CompanyName = partner.CompanyName,
                FormattedDocument = partner.FormattedDocument,
                PersonType = partner.PersonType,
                Email = partner.Email.Address,
                HasCorporateEmail = partner.HasCorporateEmail,
                Phone = partner.Phone,
                Address = new AddressResponse
                {
                    ZipCode = partner.ZipCode,
                    State = partner.State,
                    City = partner.City,
                    Street = partner.Street,
                    Number = partner.Number,
                    Neighborhood = partner.Neighborhood
                },
                Complement = partner.Complement,
                Observations = partner.Observations,
                CreatedAt = partner.CreatedAt,
                UpdatedAt = partner.UpdatedAt
            };
        }
    }
}