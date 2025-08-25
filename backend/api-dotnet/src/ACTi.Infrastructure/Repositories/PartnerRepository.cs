using ACTi.Domain.Entities;
using ACTi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ACTi.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ACTi.Infrastructure.Repositories
{
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

        public async Task<Partner> AddAsync(Partner partner, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(partner);

            var entry = await _partners.AddAsync(partner, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return entry.Entity;
        }

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

        public async Task<Partner?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _partners
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Partner?> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return null;

            var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            return await _partners
                .AsNoTracking()
                .Where(p => p.Cnpj != null && p.Cnpj.Number == cleanCnpj)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Partner?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return null;

            var cleanCpf = cpf.Replace(".", "").Replace("-", "");

            return await _partners
                .AsNoTracking()
                .Where(p => p.Cpf != null && p.Cpf.Number == cleanCpf)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Partner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _partners
                .AsNoTracking()
                .Where(p => p.Email.Address.ToLower() == email.ToLower())
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<Partner>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _partners
                .AsNoTracking()
                .OrderBy(p => p.CompanyName)
                .Skip(skip)
                .Take(Math.Min(take, 100))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Partner>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<Partner>();

            var searchTerm = name.Trim().ToLower();

            return await _partners
                .AsNoTracking()
                .Where(p => p.CompanyName.ToLower().Contains(searchTerm))
                .OrderBy(p => p.CompanyName)
                .Take(50)
                .ToListAsync(cancellationToken);
        }

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

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _partners
                .AsNoTracking()
                .AnyAsync(p => p.Id == id, cancellationToken);
        }

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

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _partners.CountAsync(cancellationToken);
        }

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
