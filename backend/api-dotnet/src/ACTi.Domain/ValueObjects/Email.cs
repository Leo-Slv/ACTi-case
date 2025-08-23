using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ACTi.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um Email válido
    /// Garante que só existam emails válidos no sistema
    /// </summary>
    public class Email
    {
        public string Address { get; private set; }

        // Construtor privado - só cria via método estático
        private Email(string address)
        {
            Address = address.ToLowerInvariant(); // Sempre minúsculo
        }

        /// <summary>
        /// Cria um Email válido ou lança exceção
        /// </summary>
        /// <param name="email">Endereço de email</param>
        /// <returns>Email válido</returns>
        /// <exception cref="ArgumentException">Quando email é inválido</exception>
        /// <example>
        /// <code>
        /// var email = Email.Create("joao@empresa.com.br");
        /// Console.WriteLine(email.IsCorporate); // true
        /// </code>
        /// </example>
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser nulo ou vazio");

            // Remove espaços
            email = email.Trim().ToLowerInvariant();

            // Validações básicas
            ValidateBasicFormat(email);

            // Validação com regex
            if (!ValidateWithRegex(email))
                throw new ArgumentException("Formato de email inválido");

            return new Email(email);
        }

        /// <summary>
        /// Domínio do email (parte após @)
        /// </summary>
        public string Domain => Address.Split('@')[1];

        /// <summary>
        /// Parte local do email (antes do @)
        /// </summary>
        public string LocalPart => Address.Split('@')[0];

        /// <summary>
        /// Verifica se é email corporativo (não Gmail, Hotmail, etc)
        /// </summary>
        public bool IsCorporate => !IsPersonalEmail();

        /// <summary>
        /// Validações básicas de formato
        /// </summary>
        /// <param name="email">Email para validação</param>
        private static void ValidateBasicFormat(string email)
        {
            if (email.Length > 254) // Limite técnico RFC 5321
                throw new ArgumentException("Email muito longo (máximo 254 caracteres)");

            if (email.Length < 5) // mínimo: a@b.c
                throw new ArgumentException("Email muito curto");

            if (!email.Contains("@"))
                throw new ArgumentException("Email deve conter @");

            if (email.Count(c => c == '@') != 1)
                throw new ArgumentException("Email deve conter exatamente um @");

            if (email.Contains(".."))
                throw new ArgumentException("Email não pode ter pontos consecutivos");

            if (email.StartsWith(".") || email.EndsWith("."))
                throw new ArgumentException("Email não pode começar ou terminar com ponto");

            if (email.StartsWith("@") || email.EndsWith("@"))
                throw new ArgumentException("Email não pode começar ou terminar com @");
        }

        /// <summary>
        /// Validação com regex (padrão mais rigoroso)
        /// </summary>
        /// <param name="email">Email para validação</param>
        /// <returns>True se válido, false caso contrário</returns>
        private static bool ValidateWithRegex(string email)
        {
            // Regex simplificado mas eficiente
            var regex = new Regex(
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                RegexOptions.IgnoreCase
            );

            return regex.IsMatch(email);
        }

        /// <summary>
        /// Verifica se é email pessoal (Gmail, Hotmail, etc)
        /// </summary>
        /// <returns>True se for email pessoal, false se corporativo</returns>
        private bool IsPersonalEmail()
        {
            var personalDomains = new[]
            {
                "gmail.com", "hotmail.com", "yahoo.com", "outlook.com",
                "live.com", "icloud.com", "terra.com.br", "uol.com.br",
                "ig.com.br", "bol.com.br", "r7.com", "zipmail.com.br"
            };

            return personalDomains.Contains(Domain.ToLower());
        }

        #region Equality & ToString

        public override bool Equals(object obj)
        {
            return obj is Email otherEmail && Address == otherEmail.Address;
        }

        public override int GetHashCode()
        {
            return Address?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Email left, Email right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Email left, Email right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Address;
        }

        /// <summary>
        /// Conversão implícita para string (facilita uso)
        /// </summary>
        /// <param name="email">Instância de Email</param>
        public static implicit operator string(Email email)
        {
            return email?.Address;
        }

        #endregion
    }
}