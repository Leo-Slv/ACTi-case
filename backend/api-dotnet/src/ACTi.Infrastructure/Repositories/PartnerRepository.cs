using ACTi.Domain.Entities;
using ACTi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ACTi.Infrastructure.Repositories
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

    /// <summary>
    /// Implementação concreta do repositório de Partner
    /// Responsável por operações de persistência usando Entity Framework
    /// </summary>
    public class PartnerRepository : IPartnerRepository
    {
        private readonly ACTiDbContext _context;
        private readonly DbSet<Partner> _partners;

        public PartnerRepository(ACTiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _partners = _context.Partners;
        }

        #region Write Operations (Commands)

        /// <summary>
        /// Adiciona um novo parceiro
        /// </summary>
        /// <param name="partner">Parceiro a ser adicionado</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Parceiro adicionado com ID preenchido</returns>
        public async Task<Partner> AddAsync(Partner partner, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(partner);

            var entry = await _partners.AddAsync(partner, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return entry.Entity;
        }

        /// <summary>
        /// Atualiza um parceiro existente
        /// </summary>
        /// <param name="partner">Parceiro com dados atualizados</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Parceiro atualizado</returns>
        /// <exception cref="InvalidOperationException">Quando parceiro não existe</exception>
        public async Task<Partner> UpdateAsync(Partner partner, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(partner);

            var exists = await ExistsAsync(partner.Id, cancellationToken);
            if (!exists)
                throw new InvalidOperationException($"Partner com ID {partner.Id} não encontrado");

            _partners.Update(partner);
            await _context.SaveChangesAsync(cancellationToken);

            return partner;
        }

        /// <summary>
        /// Remove um parceiro por ID
        /// </summary>
        /// <param name="id">ID do parceiro</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <exception cref="InvalidOperationException">Quando parceiro não existe</exception>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var partner = await GetByIdAsync(id, cancellationToken);

            if (partner == null)
                throw new InvalidOperationException($"Partner com ID {id} não encontrado");

            _partners.Remove(partner);
            await _context.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Read Operations (Queries)

        /// <summary>
        /// Busca parceiro por ID
        /// </summary>
        /// <param name="id">ID do parceiro</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Parceiro encontrado ou null</returns>
        public async Task<Partner?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _partners
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        /// <summary>
        /// Busca parceiro por CNPJ
        /// </summary>
        /// <param name="cnpj">CNPJ (apenas números)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Parceiro encontrado ou null</returns>
        public async Task<Partner?> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return null;

            // Limpar formatação do CNPJ para busca
            var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            return await _partners
                .AsNoTracking()
                .Where(p => p.Cnpj != null && p.Cnpj.Number == cleanCnpj)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Busca parceiro por CPF
        /// </summary>
        /// <param name="cpf">CPF (apenas números)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Parceiro encontrado ou null</returns>
        public async Task<Partner?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return null;

            // Limpar formatação do CPF para busca
            var cleanCpf = cpf.Replace(".", "").Replace("-", "");

            return await _partners
                .AsNoTracking()
                .Where(p => p.Cpf != null && p.Cpf.Number == cleanCpf)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Busca parceiro por email
        /// </summary>
        /// <param name="email">Email do parceiro</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Parceiro encontrado ou null</returns>
        public async Task<Partner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _partners
                .AsNoTracking()
                .Where(p => p.Email.Address.ToLower() == email.ToLower())
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Busca todos os parceiros com paginação
        /// </summary>
        /// <param name="skip">Registros a pular</param>
        /// <param name="take">Registros a retornar</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de parceiros</returns>
        public async Task<IEnumerable<Partner>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _partners
                .AsNoTracking()
                .OrderBy(p => p.CompanyName)
                .Skip(skip)
                .Take(Math.Min(take, 100)) // Máximo 100 registros por consulta
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca parceiros por nome/razão social (busca parcial)
        /// </summary>
        /// <param name="name">Parte do nome a buscar</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de parceiros encontrados</returns>
        public async Task<IEnumerable<Partner>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<Partner>();

            var searchTerm = name.Trim().ToLower();

            return await _partners
                .AsNoTracking()
                .Where(p => p.CompanyName.ToLower().Contains(searchTerm))
                .OrderBy(p => p.CompanyName)
                .Take(50) // Limitar resultados de busca
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca parceiros por localização
        /// </summary>
        /// <param name="state">Estado (UF)</param>
        /// <param name="city">Cidade (opcional)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de parceiros da localização</returns>
        public async Task<IEnumerable<Partner>> GetByLocationAsync(string state, string? city = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(state))
                return Enumerable.Empty<Partner>();

            var query = _partners.AsNoTracking()
                .Where(p => p.State.ToUpper() == state.ToUpper());

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(p => p.City.ToLower().Contains(city.ToLower()));
            }

            return await query
                .OrderBy(p => p.City)
                .ThenBy(p => p.CompanyName)
                .Take(100)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region Existence Checks

        /// <summary>
        /// Verifica se parceiro existe por ID
        /// </summary>
        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _partners
                .AsNoTracking()
                .AnyAsync(p => p.Id == id, cancellationToken);
        }

        /// <summary>
        /// Verifica se CNPJ já existe
        /// </summary>
        /// <param name="cnpj">CNPJ a verificar</param>
        /// <param name="excludeId">ID a excluir da verificação (para updates)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>True se CNPJ já existe</returns>
        public async Task<bool> CnpjExistsAsync(string cnpj, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            var query = _partners.AsNoTracking()
                .Where(p => p.Cnpj != null && p.Cnpj.Number == cleanCnpj);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        /// <summary>
        /// Verifica se CPF já existe
        /// </summary>
        public async Task<bool> CpfExistsAsync(string cpf, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            var cleanCpf = cpf.Replace(".", "").Replace("-", "");

            var query = _partners.AsNoTracking()
                .Where(p => p.Cpf != null && p.Cpf.Number == cleanCpf);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        /// <summary>
        /// Verifica se email já existe
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var query = _partners.AsNoTracking()
                .Where(p => p.Email.Address.ToLower() == email.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        #endregion

        #region Counters

        /// <summary>
        /// Conta total de parceiros
        /// </summary>
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _partners.CountAsync(cancellationToken);
        }

        /// <summary>
        /// Conta parceiros por estado
        /// </summary>
        public async Task<int> CountByStateAsync(string state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(state))
                return 0;

            return await _partners
                .AsNoTracking()
                .CountAsync(p => p.State.ToUpper() == state.ToUpper(), cancellationToken);
        }

        #endregion
    }
}