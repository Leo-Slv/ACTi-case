// ACTi.Application/Handlers/CreatePartnerHandler.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using ACTi.Application.Commands;
using ACTi.Application.DTOs.Responses;
using ACTi.Domain.Entities;
using MediatR;

namespace ACTi.Application.Handlers
{
    /// <summary>
    /// Handler responsável por processar o comando de criação de parceiro
    /// Implementa a lógica de negócio e orquestração
    /// </summary>
    public class CreatePartnerHandler : IRequestHandler<CreatePartnerCommand, PartnerResponse>
    {
        /// <summary>
        /// Processa o comando de criar parceiro
        /// </summary>
        /// <param name="request">Comando com dados do parceiro</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta com dados do parceiro criado</returns>
        public async Task<PartnerResponse> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
        {
            // Validar entrada
            ValidateRequest(request);

            // Criar entidade Partner baseada no tipo de personalidade
            var partner = CreatePartnerEntity(request);

            // TODO: Salvar no banco via Repository (implementaremos na Infrastructure)
            // await _partnerRepository.AddAsync(partner, cancellationToken);

            // Por enquanto, simular ID (quando implementarmos Repository, vem do banco)
            var partnerId = GenerateTemporaryId();

            // Mapear para DTO de resposta
            var response = MapToResponse(partner, partnerId);

            return response;
        }

        /// <summary>
        /// Valida os dados da requisição
        /// </summary>
        private static void ValidateRequest(CreatePartnerCommand request)
        {
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
        }

        /// <summary>
        /// Cria a entidade Partner baseada no tipo de personalidade
        /// </summary>
        private static Partner CreatePartnerEntity(CreatePartnerCommand request)
        {
            try
            {
                if (request.PersonalityType == "J")
                {
                    // Pessoa Jurídica - usar CNPJ
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
                    // Pessoa Física - usar CPF
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
        /// Mapeia entidade Partner para DTO de resposta
        /// </summary>
        private static PartnerResponse MapToResponse(Partner partner, int partnerId)
        {
            return new PartnerResponse
            {
                Id = partnerId,
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

        /// <summary>
        /// Gera ID temporário (substituto até implementarmos Repository)
        /// </summary>
        private static int GenerateTemporaryId()
        {
            return new Random().Next(1, 1000);
        }
    }
}