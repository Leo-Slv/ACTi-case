using ACTi.Application.DTOs.Responses;
using MediatR;

namespace ACTi.Application.Commands
{
    /// <summary>
    /// Command que representa a intenção de criar um novo parceiro
    /// Implementa IRequest do MediatR para CQRS
    /// </summary>
    public class CreatePartnerCommand : IRequest<PartnerResponse>
    {
        /// <summary>
        /// Tipo de personalidade: "F" para Física, "J" para Jurídica
        /// </summary>
        public string PersonalityType { get; set; } = string.Empty;

        /// <summary>
        /// Razão social (PJ) ou nome completo (PF)
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// CNPJ (para PJ) ou CPF (for PF) - com ou sem formatação
        /// </summary>
        public string Document { get; set; } = string.Empty;

        /// <summary>
        /// CEP do endereço - com ou sem formatação
        /// </summary>
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Estado (UF)
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Município/Cidade
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Logradouro (rua, avenida, etc)
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Número do endereço
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Bairro
        /// </summary>
        public string Neighborhood { get; set; } = string.Empty;

        /// <summary>
        /// Email de contato
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefone de contato - com ou sem formatação
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Complemento do endereço (opcional)
        /// </summary>
        public string? Complement { get; set; }

        /// <summary>
        /// Observações gerais (opcional)
        /// </summary>
        public string? Observation { get; set; }

        /// <summary>
        /// Construtor para criação via mapping
        /// </summary>
        public CreatePartnerCommand() { }

        /// <summary>
        /// Construtor com parâmetros principais (para testes)
        /// </summary>
        public CreatePartnerCommand(
            string personalityType,
            string companyName,
            string document,
            string email,
            string zipCode,
            string state,
            string city,
            string street,
            string number,
            string neighborhood,
            string phone,
            string? complement = null,
            string? observation = null)
        {
            PersonalityType = personalityType;
            CompanyName = companyName;
            Document = document;
            Email = email;
            ZipCode = zipCode;
            State = state;
            City = city;
            Street = street;
            Number = number;
            Neighborhood = neighborhood;
            Phone = phone;
            Complement = complement;
            Observation = observation;
        }
    }
}