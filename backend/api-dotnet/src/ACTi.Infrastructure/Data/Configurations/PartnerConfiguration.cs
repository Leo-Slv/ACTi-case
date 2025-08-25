using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ACTi.Domain.Entities;
using ACTi.Domain.ValueObjects;

namespace ACTi.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuração EF Core para entidade Partner
    /// Define mapeamento, índices, constraints e conversões
    /// </summary>
    public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
    {
        public void Configure(EntityTypeBuilder<Partner> builder)
        {
            ConfigureTable(builder);
            ConfigureProperties(builder);
            ConfigureValueObjects(builder);
            ConfigureIndexes(builder);
            ConfigureConstraints(builder);
        }

        /// <summary>
        /// Configuração da tabela
        /// </summary>
        private static void ConfigureTable(EntityTypeBuilder<Partner> builder)
        {
            builder.ToTable("partners");

            builder.HasKey(p => p.Id);

            // Configurar ID como identity
            builder.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
        }

        /// <summary>
        /// Configuração das propriedades básicas
        /// </summary>
        private static void ConfigureProperties(EntityTypeBuilder<Partner> builder)
        {
            // CompanyName
            builder.Property(p => p.CompanyName)
                .HasColumnName("company_name")
                .HasMaxLength(200)
                .IsRequired()
                .HasComment("Razão social (PJ) ou nome completo (PF)");

            // Endereço
            builder.Property(p => p.ZipCode)
                .HasColumnName("zip_code")
                .HasMaxLength(8)
                .IsRequired()
                .HasComment("CEP apenas números");

            builder.Property(p => p.State)
                .HasColumnName("state")
                .HasMaxLength(2)
                .IsRequired()
                .HasComment("Estado (UF)");

            builder.Property(p => p.City)
                .HasColumnName("city")
                .HasMaxLength(100)
                .IsRequired()
                .HasComment("Município/Cidade");

            builder.Property(p => p.Street)
                .HasColumnName("street")
                .HasMaxLength(200)
                .IsRequired()
                .HasComment("Logradouro");

            builder.Property(p => p.Number)
                .HasColumnName("number")
                .HasMaxLength(10)
                .IsRequired()
                .HasComment("Número do endereço");

            builder.Property(p => p.Neighborhood)
                .HasColumnName("neighborhood")
                .HasMaxLength(100)
                .IsRequired()
                .HasComment("Bairro");

            builder.Property(p => p.Complement)
                .HasColumnName("complement")
                .HasMaxLength(100)
                .HasComment("Complemento do endereço");

            // Contato
            builder.Property(p => p.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20)
                .IsRequired()
                .HasComment("Telefone de contato");

            builder.Property(p => p.Observations)
                .HasColumnName("observations")
                .HasMaxLength(500)
                .HasComment("Observações gerais");

            // Auditoria
            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasComment("Data de criação do registro");

            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasComment("Data da última atualização");
        }

        /// <summary>
        /// Configuração dos Value Objects (CNPJ, CPF, Email)
        /// </summary>
        private static void ConfigureValueObjects(EntityTypeBuilder<Partner> builder)
        {
            // Configuração do Value Object Email
            builder.Property(p => p.Email)
                .HasColumnName("email")
                .HasMaxLength(254)
                .IsRequired()
                .HasComment("Email de contato")
                .HasConversion(
                    email => email.Address,  // Para o banco
                    emailString => Email.Create(emailString)  // Do banco
                );

            // Configuração do Value Object CNPJ (nullable)
            builder.Property(p => p.Cnpj)
                .HasColumnName("cnpj")
                .HasMaxLength(14)
                .HasComment("CNPJ para pessoa jurídica (apenas números)")
                .HasConversion(
                    cnpj => cnpj != null ? cnpj.Number : null,  // Para o banco
                    cnpjString => cnpjString != null ? Cnpj.Create(cnpjString) : null  // Do banco
                );

            // Configuração do Value Object CPF (nullable)
            builder.Property(p => p.Cpf)
                .HasColumnName("cpf")
                .HasMaxLength(11)
                .HasComment("CPF para pessoa física (apenas números)")
                .HasConversion(
                    cpf => cpf != null ? cpf.Number : null,  // Para o banco
                    cpfString => cpfString != null ? Cpf.Create(cpfString) : null  // Do banco
                );
        }

        /// <summary>
        /// Configuração dos índices para performance
        /// </summary>
        private static void ConfigureIndexes(EntityTypeBuilder<Partner> builder)
        {
            // Índice único para CNPJ (quando não for nulo)
            builder.HasIndex(p => p.Cnpj)
                .IsUnique()
                .HasDatabaseName("IX_partners_cnpj")
                .HasFilter("cnpj IS NOT NULL");

            // Índice único para CPF (quando não for nulo)
            builder.HasIndex(p => p.Cpf)
                .IsUnique()
                .HasDatabaseName("IX_partners_cpf")
                .HasFilter("cpf IS NOT NULL");

            // Índice único para email
            builder.HasIndex(p => p.Email)
                .IsUnique()
                .HasDatabaseName("IX_partners_email");

            // Índice composto para busca por localização
            builder.HasIndex(p => new { p.State, p.City })
                .HasDatabaseName("IX_partners_location");

            // Índice para ordenação por data de criação
            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_partners_created_at");
        }

        /// <summary>
        /// Configuração de constraints e validações de banco
        /// </summary>
        private static void ConfigureConstraints(EntityTypeBuilder<Partner> builder)
        {
            // Check constraint: deve ter CNPJ OU CPF (não ambos, não nenhum)
            builder.HasCheckConstraint(
                "CK_partners_document_type",
                "(cnpj IS NOT NULL AND cpf IS NULL) OR (cnpj IS NULL AND cpf IS NOT NULL)"
            );

            // Check constraint: UF deve ter exatamente 2 caracteres
            builder.HasCheckConstraint(
                "CK_partners_state_length",
                "LENGTH(state) = 2"
            );

            // Check constraint: CEP deve ter exatamente 8 dígitos
            builder.HasCheckConstraint(
                "CK_partners_zipcode_format",
                "zip_code ~ '^[0-9]{8}$'"
            );

            // Check constraint: CNPJ deve ter 14 dígitos quando não nulo
            builder.HasCheckConstraint(
                "CK_partners_cnpj_format",
                "cnpj IS NULL OR (cnpj ~ '^[0-9]{14}$')"
            );

            // Check constraint: CPF deve ter 11 dígitos quando não nulo
            builder.HasCheckConstraint(
                "CK_partners_cpf_format",
                "cpf IS NULL OR (cpf ~ '^[0-9]{11}$')"
            );

            // Check constraint: email deve ter formato válido básico
            builder.HasCheckConstraint(
                "CK_partners_email_format",
                "email ~ '^[^@]+@[^@]+\\.[^@]+$'"
            );

            // Check constraint: auditoria - UpdatedAt >= CreatedAt
            builder.HasCheckConstraint(
                "CK_partners_audit_dates",
                "updated_at >= created_at"
            );
        }
    }
}