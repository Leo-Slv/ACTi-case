using ACTi.Application.Repositories;
using ACTi.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ACTi.Application.Repositories
{
    /// <summary>
    /// Interface para repositório de Partner
    /// Define contratos para operações de persistência
    /// </summary>
    public interface IPartnerRepository
    {
        // Comandos (Write operations)
        Task<Partner> AddAsync(Partner partner, CancellationToken cancellationToken = default);
        Task<Partner> UpdateAsync(Partner partner, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Consultas (Read operations)
        Task<Partner?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Partner?> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
        Task<Partner?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
        Task<Partner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        // Consultas avançadas
        Task<IEnumerable<Partner>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<Partner>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Partner>> GetByLocationAsync(string state, string? city = null, CancellationToken cancellationToken = default);

        // Verificações de existência
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> CnpjExistsAsync(string cnpj, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<bool> CpfExistsAsync(string cpf, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);

        // Contadores
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountByStateAsync(string state, CancellationToken cancellationToken = default);
    }
}
