using System;
using System.Linq;

namespace ACTi.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um CPF válido
    /// Garante que só existam CPFs válidos no sistema
    /// </summary>
    public class Cpf
    {
        public string Number { get; private set; }

        // Construtor privado - só cria via método estático
        private Cpf(string number)
        {
            Number = number;
        }

        /// <summary>
        /// Cria um CPF válido ou lança exceção
        /// </summary>
        /// <param name="cpf">CPF com ou sem formatação</param>
        /// <returns>CPF válido</returns>
        /// <exception cref="ArgumentException">Quando CPF é inválido</exception>
        /// <example>
        /// <code>
        /// var cpf = Cpf.Create("123.456.789-09");
        /// Console.WriteLine(cpf.Formatted); // "123.456.789-09"
        /// </code>
        /// </example>
        public static Cpf Create(string cpf)
        {
            // Remove formatação (pontos, hífens)
            var cleanCpf = CleanCpf(cpf);

            // Validações básicas
            ValidateBasicFormat(cleanCpf);

            // Validação do algoritmo oficial
            if (!ValidateAlgorithm(cleanCpf))
                throw new ArgumentException("CPF inválido: dígitos verificadores incorretos");

            return new Cpf(cleanCpf);
        }

        /// <summary>
        /// Retorna CPF formatado: 123.456.789-09
        /// </summary>
        public string Formatted => FormatCpf(Number);

        /// <summary>
        /// Retorna apenas os dígitos do CPF
        /// </summary>
        public string OnlyNumbers => Number;

        /// <summary>
        /// Remove toda formatação do CPF
        /// </summary>
        /// <param name="cpf">CPF com formatação</param>
        /// <returns>CPF apenas com números</returns>
        private static string CleanCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException("CPF não pode ser nulo ou vazio");

            return cpf.Replace(".", "")
                     .Replace("-", "")
                     .Replace(" ", "")
                     .Trim();
        }

        /// <summary>
        /// Valida formato básico (tamanho, só números)
        /// </summary>
        /// <param name="cleanCpf">CPF limpo para validação</param>
        private static void ValidateBasicFormat(string cleanCpf)
        {
            if (cleanCpf.Length != 11)
                throw new ArgumentException("CPF deve ter exatamente 11 dígitos");

            if (!cleanCpf.All(char.IsDigit))
                throw new ArgumentException("CPF deve conter apenas números");

            // Verifica sequências inválidas (00000000000, 11111111111, etc)
            if (cleanCpf.All(c => c == cleanCpf[0]))
                throw new ArgumentException("CPF não pode ter todos os dígitos iguais");
        }

        /// <summary>
        /// Valida CPF usando algoritmo oficial da Receita Federal
        /// </summary>
        /// <param name="cpf">CPF para validação</param>
        /// <returns>True se válido, false caso contrário</returns>
        private static bool ValidateAlgorithm(string cpf)
        {
            // Calcula primeiro dígito verificador
            int firstSum = 0;
            for (int i = 0; i < 9; i++)
            {
                firstSum += int.Parse(cpf[i].ToString()) * (10 - i);
            }

            int firstRemainder = firstSum % 11;
            int firstDigit = firstRemainder < 2 ? 0 : 11 - firstRemainder;

            // Verifica primeiro dígito
            if (int.Parse(cpf[9].ToString()) != firstDigit)
                return false;

            // Calcula segundo dígito verificador
            int secondSum = 0;
            for (int i = 0; i < 10; i++)
            {
                secondSum += int.Parse(cpf[i].ToString()) * (11 - i);
            }

            int secondRemainder = secondSum % 11;
            int secondDigit = secondRemainder < 2 ? 0 : 11 - secondRemainder;

            // Verifica segundo dígito
            return int.Parse(cpf[10].ToString()) == secondDigit;
        }

        /// <summary>
        /// Formata CPF: 12345678909 → 123.456.789-09
        /// </summary>
        /// <param name="cpf">CPF sem formatação</param>
        /// <returns>CPF formatado</returns>
        private static string FormatCpf(string cpf)
        {
            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }

        #region Equality & ToString

        public override bool Equals(object obj)
        {
            return obj is Cpf otherCpf && Number == otherCpf.Number;
        }

        public override int GetHashCode()
        {
            return Number?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Cpf left, Cpf right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Cpf left, Cpf right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Formatted;
        }

        /// <summary>
        /// Conversão implícita para string (facilita uso)
        /// </summary>
        /// <param name="cpf">Instância de CPF</param>
        public static implicit operator string(Cpf cpf)
        {
            return cpf?.Number;
        }

        #endregion
    }
}