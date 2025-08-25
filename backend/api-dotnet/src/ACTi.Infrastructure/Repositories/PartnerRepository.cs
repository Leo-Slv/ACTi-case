using ACTi.Application.Repositories;
using ACTi.Domain.Entities;
using ACTi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ACTi.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação concreta do repositório de Partner para SQL Server
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
        public async Task<Partner?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _partners
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        /// <summary>
        /// Busca parceiro por CNPJ usando SQL Server - Esquema DBO
        /// </summary>
        public async Task<Partner?> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return null;

            // Limpar formatação do CNPJ para busca
            var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            // SQL Server com esquema dbo (não acti)
            return await _partners
                .FromSqlRaw("SELECT * FROM [dbo].[partners] WHERE [cnpj] = {0}", cleanCnpj)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Busca parceiro por CPF usando SQL Server - Esquema DBO
        /// </summary>
        public async Task<Partner?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return null;

            // Limpar formatação do CPF para busca
            var cleanCpf = cpf.Replace(".", "").Replace("-", "");

            // SQL Server com esquema dbo (não acti)
            return await _partners
                .FromSqlRaw("SELECT * FROM [dbo].[partners] WHERE [cpf] = {0}", cleanCpf)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Busca parceiro por email
        /// </summary>
        public async Task<Partner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _partners
                .FromSqlRaw("SELECT * FROM [dbo].[partners] WHERE LOWER([email]) = LOWER({0})", email)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Busca todos os parceiros com paginação
        /// </summary>
        public async Task<IEnumerable<Partner>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _partners
                .AsNoTracking()
                .OrderBy(p => p.CompanyName)
                .Skip(skip)
                .Take(Math.Min(take, 100))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca parceiros por nome/razão social
        /// </summary>
        public async Task<IEnumerable<Partner>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<Partner>();

            var searchTerm = $"%{name.Trim()}%";

            return await _partners
                .FromSqlRaw("SELECT TOP 50 * FROM [dbo].[partners] WHERE LOWER([company_name]) LIKE LOWER({0}) ORDER BY [company_name]", searchTerm)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca parceiros por localização
        /// </summary>
        public async Task<IEnumerable<Partner>> GetByLocationAsync(string state, string? city = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(state))
                return Enumerable.Empty<Partner>();

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityPattern = $"%{city}%";
                return await _partners
                    .FromSqlRaw("SELECT TOP 100 * FROM [dbo].[partners] WHERE UPPER([state]) = UPPER({0}) AND LOWER([city]) LIKE LOWER({1}) ORDER BY [city], [company_name]", state, cityPattern)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            else
            {
                return await _partners
                    .FromSqlRaw("SELECT TOP 100 * FROM [dbo].[partners] WHERE UPPER([state]) = UPPER({0}) ORDER BY [city], [company_name]", state)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
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
        /// Verifica se CNPJ já existe usando SQL Server - Esquema DBO
        /// </summary>
        public async Task<bool> CnpjExistsAsync(string cnpj, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            if (excludeId.HasValue)
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM [dbo].[partners] WHERE [cnpj] = {0} AND [id] != {1}", cleanCnpj, excludeId.Value)
                    .FirstOrDefaultAsync(cancellationToken);
                return result > 0;
            }
            else
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM [dbo].[partners] WHERE [cnpj] = {0}", cleanCnpj)
                    .FirstOrDefaultAsync(cancellationToken);
                return result > 0;
            }
        }

        /// <summary>
        /// Verifica se CPF já existe usando SQL Server - Esquema DBO
        /// </summary>
        public async Task<bool> CpfExistsAsync(string cpf, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            var cleanCpf = cpf.Replace(".", "").Replace("-", "");

            if (excludeId.HasValue)
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM [dbo].[partners] WHERE [cpf] = {0} AND [id] != {1}", cleanCpf, excludeId.Value)
                    .FirstOrDefaultAsync(cancellationToken);
                return result > 0;
            }
            else
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM [dbo].[partners] WHERE [cpf] = {0}", cleanCpf)
                    .FirstOrDefaultAsync(cancellationToken);
                return result > 0;
            }
        }

        /// <summary>
        /// Verifica se email já existe usando SQL Server - Esquema DBO
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (excludeId.HasValue)
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM [dbo].[partners] WHERE LOWER([email]) = LOWER({0}) AND [id] != {1}", email, excludeId.Value)
                    .FirstOrDefaultAsync(cancellationToken);
                return result > 0;
            }
            else
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM [dbo].[partners] WHERE LOWER([email]) = LOWER({0})", email)
                    .FirstOrDefaultAsync(cancellationToken);
                return result > 0;
            }
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

            var result = await _context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM [dbo].[partners] WHERE UPPER([state]) = UPPER({0})", state)
                .FirstOrDefaultAsync(cancellationToken);

            return result;
        }

        #endregion
    }
}