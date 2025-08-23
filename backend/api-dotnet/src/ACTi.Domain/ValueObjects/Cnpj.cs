using System;
using System.Linq;

namespace ACTi.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um CNPJ válido
    /// Garante que só existam CNPJs válidos no sistema
    /// </summary>
    public class Cnpj
    {
        public string Number { get; private set; }

        // Construtor privado - só cria via método estático
        private Cnpj(string number)
        {
            Number = number;
        }

        /// <summary>
        /// Cria um CNPJ válido ou lança exceção
        /// </summary>
        /// <param name="cnpj">CNPJ com ou sem formatação</param>
        /// <returns>CNPJ válido</returns>
        /// <exception cref="ArgumentException">Quando CNPJ é inválido</exception>
        /// <example>
        /// <code>
        /// var cnpj = Cnpj.Create("12.345.678/0001-90");
        /// Console.WriteLine(cnpj.Formatted); // "12.345.678/0001-90"
        /// </code>
        /// </example>
        public static Cnpj Create(string cnpj)
        {
            // Remove formatação (pontos, barras, hífens)
            var cleanCnpj = CleanCnpj(cnpj);

            // Validações básicas
            ValidateBasicFormat(cleanCnpj);

            // Validação do algoritmo oficial
            if (!ValidateAlgorithm(cleanCnpj))
                throw new ArgumentException("CNPJ inválido: dígitos verificadores incorretos");

            return new Cnpj(cleanCnpj);
        }

        /// <summary>
        /// Retorna CNPJ formatado: 12.345.678/0001-90
        /// </summary>
        public string Formatted => FormatCnpj(Number);

        /// <summary>
        /// Retorna apenas os dígitos do CNPJ
        /// </summary>
        public string OnlyNumbers => Number;

        /// <summary>
        /// Remove toda formatação do CNPJ
        /// </summary>
        /// <param name="cnpj">CNPJ com formatação</param>
        /// <returns>CNPJ apenas com números</returns>
        private static string CleanCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                throw new ArgumentException("CNPJ não pode ser nulo ou vazio");

            return cnpj.Replace(".", "")
                      .Replace("/", "")
                      .Replace("-", "")
                      .Replace(" ", "")
                      .Trim();
        }

        /// <summary>
        /// Valida formato básico (tamanho, só números)
        /// </summary>
        /// <param name="cleanCnpj">CNPJ limpo para validação</param>
        private static void ValidateBasicFormat(string cleanCnpj)
        {
            if (cleanCnpj.Length != 14)
                throw new ArgumentException("CNPJ deve ter exatamente 14 dígitos");

            if (!cleanCnpj.All(char.IsDigit))
                throw new ArgumentException("CNPJ deve conter apenas números");

            // Verifica sequências inválidas (00000000000000, 11111111111111, etc)
            if (cleanCnpj.All(c => c == cleanCnpj[0]))
                throw new ArgumentException("CNPJ não pode ter todos os dígitos iguais");
        }

        /// <summary>
        /// Valida CNPJ usando algoritmo oficial da Receita Federal
        /// </summary>
        /// <param name="cnpj">CNPJ para validação</param>
        /// <returns>True se válido, false caso contrário</returns>
        private static bool ValidateAlgorithm(string cnpj)
        {
            // Pesos para primeiro dígito verificador
            int[] firstWeights = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            // Pesos para segundo dígito verificador  
            int[] secondWeights = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            // Calcula primeiro dígito verificador
            int firstSum = 0;
            for (int i = 0; i < 12; i++)
            {
                firstSum += int.Parse(cnpj[i].ToString()) * firstWeights[i];
            }

            int firstRemainder = firstSum % 11;
            int firstDigit = firstRemainder < 2 ? 0 : 11 - firstRemainder;

            // Verifica primeiro dígito
            if (int.Parse(cnpj[12].ToString()) != firstDigit)
                return false;

            // Calcula segundo dígito verificador
            int secondSum = 0;
            for (int i = 0; i < 13; i++)
            {
                secondSum += int.Parse(cnpj[i].ToString()) * secondWeights[i];
            }

            int secondRemainder = secondSum % 11;
            int secondDigit = secondRemainder < 2 ? 0 : 11 - secondRemainder;

            // Verifica segundo dígito
            return int.Parse(cnpj[13].ToString()) == secondDigit;
        }

        /// <summary>
        /// Formata CNPJ: 12345678000190 → 12.345.678/0001-90
        /// </summary>
        /// <param name="cnpj">CNPJ sem formatação</param>
        /// <returns>CNPJ formatado</returns>
        private static string FormatCnpj(string cnpj)
        {
            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
        }

        #region Equality & ToString

        public override bool Equals(object obj)
        {
            return obj is Cnpj otherCnpj && Number == otherCnpj.Number;
        }

        public override int GetHashCode()
        {
            return Number?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Cnpj left, Cnpj right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Cnpj left, Cnpj right)
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
        /// <param name="cnpj">Instância de CNPJ</param>
        public static implicit operator string(Cnpj cnpj)
        {
            return cnpj?.Number;
        }

        #endregion
    }
}