namespace ACTi.Infrastructure.Services.ExternalApis
{
    /// <summary>
    /// Resposta padronizada para consultas de CEP
    /// </summary>
    public class CepResponse
    {
        public string Cep { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta padronizada para consultas de CNPJ
    /// </summary>
    public class CnpjResponse
    {
        public string Cnpj { get; set; } = string.Empty;
        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Situacao { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interface para serviços de APIs externas
    /// </summary>
    public interface IExternalApiService
    {
        /// <summary>
        /// Consulta dados de endereço por CEP
        /// </summary>
        /// <param name="cep">CEP para consulta</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados do endereço ou erro</returns>
        Task<CepResponse> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consulta dados de empresa por CNPJ
        /// </summary>
        /// <param name="cnpj">CNPJ para consulta</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da empresa ou erro</returns>
        Task<CnpjResponse> ConsultarCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
    }
}