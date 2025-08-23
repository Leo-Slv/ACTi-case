// ACTi.Domain/Entities/Partner.cs
using System;
using ACTi.Domain.ValueObjects;

namespace ACTi.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um Parceiro Comercial
    /// Aggregate Root que garante consistência de dados e regras de negócio
    /// </summary>
    public class Partner
    {
        /// <summary>
        /// Identificador único do parceiro
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Razão social ou nome completo do parceiro
        /// </summary>
        public string CompanyName { get; private set; }

        // Value Objects - Garantem dados sempre válidos
        /// <summary>
        /// CNPJ para pessoa jurídica (nulo para pessoa física)
        /// </summary>
        public Cnpj Cnpj { get; private set; }

        /// <summary>
        /// CPF para pessoa física (nulo para pessoa jurídica)
        /// </summary>
        public Cpf Cpf { get; private set; }

        /// <summary>
        /// Email de contato do parceiro
        /// </summary>
        public Email Email { get; private set; }

        // Dados de endereço
        /// <summary>
        /// CEP do endereço (apenas números)
        /// </summary>
        public string ZipCode { get; private set; }

        /// <summary>
        /// Estado (UF) do endereço
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// Município/Cidade do endereço
        /// </summary>
        public string City { get; private set; }

        /// <summary>
        /// Logradouro (rua, avenida, etc)
        /// </summary>
        public string Street { get; private set; }

        /// <summary>
        /// Número do endereço
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Bairro do endereço
        /// </summary>
        public string Neighborhood { get; private set; }

        /// <summary>
        /// Complemento do endereço (opcional)
        /// </summary>
        public string Complement { get; private set; }

        // Dados de contato
        /// <summary>
        /// Telefone de contato
        /// </summary>
        public string Phone { get; private set; }

        /// <summary>
        /// Observações gerais sobre o parceiro (opcional)
        /// </summary>
        public string Observations { get; private set; }

        // Dados de auditoria
        /// <summary>
        /// Data de criação do registro
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Data da última modificação
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        // Construtor privado - só cria via factory methods
        private Partner()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cria um parceiro pessoa jurídica
        /// </summary>
        /// <param name="companyName">Razão social da empresa</param>
        /// <param name="cnpj">CNPJ da empresa</param>
        /// <param name="email">Email de contato</param>
        /// <param name="zipCode">CEP do endereço</param>
        /// <param name="state">Estado (UF)</param>
        /// <param name="city">Cidade</param>
        /// <param name="street">Logradouro</param>
        /// <param name="number">Número</param>
        /// <param name="neighborhood">Bairro</param>
        /// <param name="phone">Telefone</param>
        /// <param name="complement">Complemento (opcional)</param>
        /// <param name="observations">Observações (opcional)</param>
        /// <returns>Parceiro pessoa jurídica válido</returns>
        public static Partner CreateLegalPerson(
            string companyName,
            string cnpj,
            string email,
            string zipCode,
            string state,
            string city,
            string street,
            string number,
            string neighborhood,
            string phone,
            string complement = null,
            string observations = null)
        {
            var partner = new Partner();

            // Validações de negócio
            partner.ValidateRequiredFields(companyName, zipCode, state, city, street, number, neighborhood, phone);

            // Criar Value Objects - se inválidos, já dão erro aqui!
            partner.Cnpj = Cnpj.Create(cnpj);
            partner.Email = Email.Create(email);

            // Atribuir valores
            partner.AssignBasicData(companyName, zipCode, state, city, street, number, neighborhood, phone, complement, observations);

            return partner;
        }

        /// <summary>
        /// Cria um parceiro pessoa física
        /// </summary>
        /// <param name="fullName">Nome completo da pessoa</param>
        /// <param name="cpf">CPF da pessoa</param>
        /// <param name="email">Email de contato</param>
        /// <param name="zipCode">CEP do endereço</param>
        /// <param name="state">Estado (UF)</param>
        /// <param name="city">Cidade</param>
        /// <param name="street">Logradouro</param>
        /// <param name="number">Número</param>
        /// <param name="neighborhood">Bairro</param>
        /// <param name="phone">Telefone</param>
        /// <param name="complement">Complemento (opcional)</param>
        /// <param name="observations">Observações (opcional)</param>
        /// <returns>Parceiro pessoa física válido</returns>
        public static Partner CreateNaturalPerson(
            string fullName,
            string cpf,
            string email,
            string zipCode,
            string state,
            string city,
            string street,
            string number,
            string neighborhood,
            string phone,
            string complement = null,
            string observations = null)
        {
            var partner = new Partner();

            // Validações de negócio
            partner.ValidateRequiredFields(fullName, zipCode, state, city, street, number, neighborhood, phone);

            // Criar Value Objects - validação automática!
            partner.Cpf = Cpf.Create(cpf);
            partner.Email = Email.Create(email);

            // Atribuir valores
            partner.AssignBasicData(fullName, zipCode, state, city, street, number, neighborhood, phone, complement, observations);

            return partner;
        }

        /// <summary>
        /// Atualiza email do parceiro
        /// </summary>
        /// <param name="newEmail">Novo email</param>
        public void UpdateEmail(string newEmail)
        {
            Email = Email.Create(newEmail); // ← Validação automática!
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Atualiza telefone do parceiro
        /// </summary>
        /// <param name="newPhone">Novo telefone</param>
        public void UpdatePhone(string newPhone)
        {
            if (string.IsNullOrWhiteSpace(newPhone))
                throw new ArgumentException("Telefone é obrigatório");

            Phone = newPhone.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Atualiza endereço do parceiro
        /// </summary>
        public void UpdateAddress(string zipCode, string state, string city, string street,
                                string number, string neighborhood, string complement = null)
        {
            ValidateAddressFields(zipCode, state, city, street, number, neighborhood);

            ZipCode = zipCode?.Replace("-", "").Trim();
            State = state?.ToUpper().Trim();
            City = city?.Trim();
            Street = street?.Trim();
            Number = number?.Trim();
            Neighborhood = neighborhood?.Trim();
            Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();

            UpdatedAt = DateTime.UtcNow;
        }

        // Propriedades calculadas
        /// <summary>
        /// Verifica se o parceiro é pessoa jurídica
        /// </summary>
        public bool IsLegalPerson => Cnpj != null;

        /// <summary>
        /// Verifica se o parceiro é pessoa física
        /// </summary>
        public bool IsNaturalPerson => Cpf != null;

        /// <summary>
        /// Verifica se tem email corporativo
        /// </summary>
        public bool HasCorporateEmail => Email?.IsCorporate == true;

        /// <summary>
        /// Retorna documento formatado (CNPJ ou CPF)
        /// </summary>
        public string FormattedDocument => IsLegalPerson ? Cnpj.Formatted : Cpf?.Formatted;

        /// <summary>
        /// Retorna tipo de pessoa para exibição
        /// </summary>
        public string PersonType => IsLegalPerson ? "Pessoa Jurídica" : "Pessoa Física";

        #region Private Methods

        /// <summary>
        /// Valida campos obrigatórios
        /// </summary>
        private void ValidateRequiredFields(string companyName, string zipCode, string state,
                                          string city, string street, string number,
                                          string neighborhood, string phone)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                throw new ArgumentException("Nome/Razão social é obrigatório");

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Telefone é obrigatório");

            ValidateAddressFields(zipCode, state, city, street, number, neighborhood);
        }

        /// <summary>
        /// Valida campos de endereço
        /// </summary>
        private void ValidateAddressFields(string zipCode, string state, string city,
                                         string street, string number, string neighborhood)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("CEP é obrigatório");

            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("Estado é obrigatório");

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("Cidade é obrigatória");

            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Logradouro é obrigatório");

            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Número é obrigatório");

            if (string.IsNullOrWhiteSpace(neighborhood))
                throw new ArgumentException("Bairro é obrigatório");
        }

        /// <summary>
        /// Atribui dados básicos do parceiro
        /// </summary>
        private void AssignBasicData(string companyName, string zipCode, string state,
                                   string city, string street, string number,
                                   string neighborhood, string phone,
                                   string complement, string observations)
        {
            CompanyName = companyName.Trim();
            ZipCode = zipCode?.Replace("-", "").Trim();
            State = state?.ToUpper().Trim();
            City = city?.Trim();
            Street = street?.Trim();
            Number = number?.Trim();
            Neighborhood = neighborhood?.Trim();
            Phone = phone?.Trim();
            Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
            Observations = string.IsNullOrWhiteSpace(observations) ? null : observations.Trim();
        }

        #endregion
    }
}