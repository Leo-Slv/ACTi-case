using ACTi.Domain.Entities;
using ACTi.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ACTi.Infrastructure.Data
{
    /// <summary>
    /// Contexto principal do Entity Framework para ACTi
    /// Responsável pela configuração e mapeamento das entidades
    /// </summary>
    public class ACTiDbContext : DbContext
    {
        public ACTiDbContext(DbContextOptions<ACTiDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet para entidade Partner
        /// </summary>
        public DbSet<Partner> Partners { get; set; } = null!;

        /// <summary>
        /// Configuração do modelo de dados
        /// </summary>
        /// <param name="modelBuilder">Builder para configuração do modelo</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas as configurações de entidades
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PartnerConfiguration).Assembly);

            // Configurações globais
            ConfigureGlobalSettings(modelBuilder);
        }

        /// <summary>
        /// Configurações globais do banco de dados
        /// </summary>
        /// <param name="modelBuilder">Builder para configuração</param>
        private static void ConfigureGlobalSettings(ModelBuilder modelBuilder)
        {
            // Configurar esquema padrão
            modelBuilder.HasDefaultSchema("acti");

            // Configurar convenções de nomeação para snake_case
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Tabelas em snake_case
                entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

                // Propriedades em snake_case
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName()?.ToSnakeCase());
                }
            }
        }

        /// <summary>
        /// Override para interceptar mudanças e adicionar auditoria automática
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Número de registros afetados</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Aplicar auditoria automática
            ApplyAuditInfo();

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Override síncrono para auditoria
        /// </summary>
        /// <returns>Número de registros afetados</returns>
        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        /// <summary>
        /// Aplica informações de auditoria automaticamente
        /// </summary>
        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker.Entries<Partner>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    // Para novos registros, definir CreatedAt
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Para registros modificados, apenas UpdatedAt
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                    // Não permitir modificação do CreatedAt
                    entry.Property("CreatedAt").IsModified = false;
                }
            }
        }
    }

    /// <summary>
    /// Extensão para converter strings para snake_case
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converte PascalCase para snake_case
        /// Exemplo: "CompanyName" -> "company_name"
        /// </summary>
        /// <param name="input">String em PascalCase</param>
        /// <returns>String em snake_case</returns>
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new System.Text.StringBuilder();
            result.Append(char.ToLowerInvariant(input[0]));

            for (int i = 1; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsUpper(c))
                {
                    result.Append('_');
                    result.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}