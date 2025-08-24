using System;

namespace ACTi.Application.DTOs.Responses
{
    /// <summary>
    /// DTO para resposta com dados do parceiro
    /// Contém informações seguras para enviar ao frontend
    /// </summary>
    public class PartnerResponse
    {
        /// <summary>
        /// Identificador único do parceiro
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Razão social ou nome completo
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Documento formatado (CNPJ ou CPF com máscaras)
        /// </summary>
        public string FormattedDocument { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de pessoa ("Pessoa Física" ou "Pessoa Jurídica")
        /// </summary>
        public string PersonType { get; set; } = string.Empty;

        /// <summary>
        /// Email de contato
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o email é corporativo
        /// </summary>
        public bool HasCorporateEmail { get; set; }

        /// <summary>
        /// Telefone de contato
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Endereço completo formatado
        /// </summary>
        public AddressResponse Address { get; set; } = new();

        /// <summary>
        /// Complemento do endereço (se houver)
        /// </summary>
        public string? Complement { get; set; }

        /// <summary>
        /// Observações (se houver)
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Data de criação do registro
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO específico para dados de endereço
    /// </summary>
    public class AddressResponse
    {
        /// <summary>
        /// CEP formatado
        /// </summary>
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Estado (UF)
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Cidade
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Logradouro
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Número
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Bairro
        /// </summary>
        public string Neighborhood { get; set; } = string.Empty;

        /// <summary>
        /// Endereço completo formatado para exibição
        /// </summary>
        public string FullAddress => $"{Street}, {Number}, {Neighborhood}, {City}/{State}, CEP: {FormatZipCode()}";

        /// <summary>
        /// Formatar CEP com máscara
        /// </summary>
        private string FormatZipCode()
        {
            if (ZipCode.Length == 8)
                return $"{ZipCode.Substring(0, 5)}-{ZipCode.Substring(5, 3)}";
            return ZipCode;
        }
    }
}