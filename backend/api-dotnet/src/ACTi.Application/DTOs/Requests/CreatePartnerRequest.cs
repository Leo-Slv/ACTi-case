using System.ComponentModel.DataAnnotations;

namespace ACTi.Application.DTOs.Requests
{
    /// <summary>
    /// DTO para requisição de criação de parceiro
    /// Contém todos os dados necessários vindos do frontend
    /// </summary>
    public class CreatePartnerRequest
    {
        /// <summary>
        /// Tipo de personalidade: "F" para Física, "J" para Jurídica
        /// </summary>
        [Required(ErrorMessage = "Personalidade é obrigatória")]
        public string PersonalityType { get; set; } = string.Empty;

        /// <summary>
        /// Razão social (PJ) ou nome completo (PF)
        /// </summary>
        [Required(ErrorMessage = "Razão social/Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Razão social deve ter no máximo 200 caracteres")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// CNPJ (para PJ) ou CPF (for PF) - com ou sem formatação
        /// </summary>
        [Required(ErrorMessage = "CNPJ/CPF é obrigatório")]
        public string Document { get; set; } = string.Empty;

        /// <summary>
        /// CEP do endereço - com ou sem formatação
        /// </summary>
        [Required(ErrorMessage = "CEP é obrigatório")]
        public string ZipCode { get; set; } = string.Empty;

        /// <summary>
        /// Estado (UF)
        /// </summary>
        [Required(ErrorMessage = "UF é obrigatória")]
        [StringLength(2, ErrorMessage = "UF deve ter 2 caracteres")]
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Município/Cidade
        /// </summary>
        [Required(ErrorMessage = "Município é obrigatório")]
        [StringLength(100, ErrorMessage = "Município deve ter no máximo 100 caracteres")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Logradouro (rua, avenida, etc)
        /// </summary>
        [Required(ErrorMessage = "Logradouro é obrigatório")]
        [StringLength(200, ErrorMessage = "Logradouro deve ter no máximo 200 caracteres")]
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Número do endereço
        /// </summary>
        [Required(ErrorMessage = "Número é obrigatório")]
        [StringLength(10, ErrorMessage = "Número deve ter no máximo 10 caracteres")]
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Bairro
        /// </summary>
        [Required(ErrorMessage = "Bairro é obrigatório")]
        [StringLength(100, ErrorMessage = "Bairro deve ter no máximo 100 caracteres")]
        public string Neighborhood { get; set; } = string.Empty;

        /// <summary>
        /// Email de contato
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(254, ErrorMessage = "Email deve ter no máximo 254 caracteres")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefone de contato - com ou sem formatação
        /// </summary>
        [Required(ErrorMessage = "Telefone é obrigatório")]
        [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Complemento do endereço (opcional)
        /// </summary>
        [StringLength(100, ErrorMessage = "Complemento deve ter no máximo 100 caracteres")]
        public string? Complement { get; set; }

        /// <summary>
        /// Observações gerais (opcional)
        /// </summary>
        [StringLength(500, ErrorMessage = "Observação deve ter no máximo 500 caracteres")]
        public string? Observation { get; set; }
    }
}