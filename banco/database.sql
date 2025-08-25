-- ============================================================================
-- SCRIPT SQL SERVER - PROJETO ACTi
-- ============================================================================
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ACTiDb')
BEGIN
    CREATE DATABASE ACTiDb;
    PRINT 'Database ACTiDb criado com sucesso!';
END
ELSE
BEGIN
    PRINT 'Database ACTiDb já existe.';
END
GO
USE ACTiDb;
GO

-- ============================================================================
-- CRIAÇÃO DA TABELA PARTNERS
-- ============================================================================

-- Dropar tabela se existir (para recriar)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[partners]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[partners];
    PRINT 'Tabela partners removida.';
END
GO

-- Criar tabela partners
CREATE TABLE [dbo].[partners] (
    -- Chave primária
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Dados básicos
    [company_name] NVARCHAR(200) NOT NULL,
    
    -- Documentos (CPF ou CNPJ - mutuamente exclusivos)
    [cnpj] VARCHAR(14) NULL,                     -- CNPJ apenas números
    [cpf] VARCHAR(11) NULL,                      -- CPF apenas números
    
    -- Email
    [email] NVARCHAR(254) NOT NULL,
    
    -- Endereço
    [zip_code] VARCHAR(8) NOT NULL,              -- CEP apenas números
    [state] VARCHAR(2) NOT NULL,                 -- UF
    [city] NVARCHAR(100) NOT NULL,
    [street] NVARCHAR(200) NOT NULL,
    [number] NVARCHAR(10) NOT NULL,
    [neighborhood] NVARCHAR(100) NOT NULL,
    [complement] NVARCHAR(100) NULL,
    
    -- Contato
    [phone] NVARCHAR(20) NOT NULL,
    
    -- Observações
    [observations] NVARCHAR(500) NULL,
    
    -- Auditoria
    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

PRINT 'Tabela partners criada com sucesso!';

-- ============================================================================
-- CRIAÇÃO DE ÍNDICES
-- ============================================================================

-- Índice único para CNPJ (quando não for nulo)
CREATE UNIQUE INDEX [IX_partners_cnpj] 
ON [dbo].[partners]([cnpj]) 
WHERE [cnpj] IS NOT NULL;

-- Índice único para CPF (quando não for nulo)
CREATE UNIQUE INDEX [IX_partners_cpf] 
ON [dbo].[partners]([cpf]) 
WHERE [cpf] IS NOT NULL;

-- Índice único para email
CREATE UNIQUE INDEX [IX_partners_email] 
ON [dbo].[partners]([email]);

-- Índice composto para localização
CREATE INDEX [IX_partners_location] 
ON [dbo].[partners]([state], [city]);

-- Índice para data de criação (ordenação)
CREATE INDEX [IX_partners_created_at] 
ON [dbo].[partners]([created_at]);

-- Índice para busca por nome
CREATE INDEX [IX_partners_company_name] 
ON [dbo].[partners]([company_name]);

PRINT 'Índices criados com sucesso!';

-- ============================================================================
-- CONSTRAINTS E VALIDAÇÕES
-- ============================================================================

-- Constraint: Deve ter CNPJ OU CPF (não ambos, não nenhum)
ALTER TABLE [dbo].[partners] 
ADD CONSTRAINT [CK_partners_document_type] 
CHECK (
    ([cnpj] IS NOT NULL AND [cpf] IS NULL) OR 
    ([cnpj] IS NULL AND [cpf] IS NOT NULL)
);

-- Constraint: UF deve ter exatamente 2 caracteres
ALTER TABLE [dbo].[partners] 
ADD CONSTRAINT [CK_partners_state_length] 
CHECK (LEN([state]) = 2);

-- Constraint: CEP deve ter exatamente 8 dígitos
ALTER TABLE [dbo].[partners] 
ADD CONSTRAINT [CK_partners_zipcode_format] 
CHECK (LEN([zip_code]) = 8 AND [zip_code] NOT LIKE '%[^0-9]%');

-- Constraint: CNPJ deve ter 14 dígitos quando não nulo
ALTER TABLE [dbo].[partners] 
ADD CONSTRAINT [CK_partners_cnpj_format] 
CHECK ([cnpj] IS NULL OR (LEN([cnpj]) = 14 AND [cnpj] NOT LIKE '%[^0-9]%'));

-- Constraint: CPF deve ter 11 dígitos quando não nulo
ALTER TABLE [dbo].[partners] 
ADD CONSTRAINT [CK_partners_cpf_format] 
CHECK ([cpf] IS NULL OR (LEN([cpf]) = 11 AND [cpf] NOT LIKE '%[^0-9]%'));

-- Constraint: Email deve ter formato básico válido
ALTER TABLE [dbo].[partners] 
ADD CONSTRAINT [CK_partners_email_format] 
CHECK ([email] LIKE '%@%.%');

-- Constraint: UpdatedAt >= CreatedAt
ALTER TABLE [dbo].[partners] 
ADD CONSTRAINT [CK_partners_audit_dates] 
CHECK ([updated_at] >= [created_at]);

PRINT 'Constraints criadas com sucesso!';

-- ============================================================================
-- STORED PROCEDURE: sp_inserir_parceiro
-- ============================================================================

-- Dropar procedure se existir (para recriar)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_inserir_parceiro')
BEGIN
    DROP PROCEDURE [dbo].[sp_inserir_parceiro];
    PRINT 'Procedure sp_inserir_parceiro removida.';
END
GO

CREATE PROCEDURE [dbo].[sp_inserir_parceiro]
    @PersonalityType CHAR(1),           -- 'F' para Física, 'J' para Jurídica
    @CompanyName NVARCHAR(200),
    @Document VARCHAR(14),              -- CPF ou CNPJ (apenas números)
    @Email NVARCHAR(254),
    @ZipCode VARCHAR(8),
    @State VARCHAR(2),
    @City NVARCHAR(100),
    @Street NVARCHAR(200),
    @Number NVARCHAR(10),
    @Neighborhood NVARCHAR(100),
    @Phone NVARCHAR(20),
    @Complement NVARCHAR(100) = NULL,
    @Observations NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PartnerId INT;
    DECLARE @ErrorMessage NVARCHAR(500);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- ========================================================================
        -- VALIDAÇÕES BÁSICAS
        -- ========================================================================
        
        -- Validar PersonalityType
        IF @PersonalityType NOT IN ('F', 'J')
        BEGIN
            SET @ErrorMessage = 'Tipo de personalidade deve ser F (Física) ou J (Jurídica)';
            THROW 50001, @ErrorMessage, 1;
        END
        
        -- Validar campos obrigatórios
        IF LTRIM(RTRIM(ISNULL(@CompanyName, ''))) = ''
        BEGIN
            SET @ErrorMessage = 'Nome/Razão social é obrigatório';
            THROW 50002, @ErrorMessage, 1;
        END
        
        IF LTRIM(RTRIM(ISNULL(@Document, ''))) = ''
        BEGIN
            SET @ErrorMessage = 'CPF/CNPJ é obrigatório';
            THROW 50003, @ErrorMessage, 1;
        END
        
        IF LTRIM(RTRIM(ISNULL(@Email, ''))) = ''
        BEGIN
            SET @ErrorMessage = 'Email é obrigatório';
            THROW 50004, @ErrorMessage, 1;
        END
        
        -- Validar formato do documento
        IF @PersonalityType = 'J' AND LEN(@Document) != 14
        BEGIN
            SET @ErrorMessage = 'CNPJ deve conter exatamente 14 dígitos';
            THROW 50005, @ErrorMessage, 1;
        END
        
        IF @PersonalityType = 'F' AND LEN(@Document) != 11
        BEGIN
            SET @ErrorMessage = 'CPF deve conter exatamente 11 dígitos';
            THROW 50006, @ErrorMessage, 1;
        END
        
        -- Validar se documento contém apenas números
        IF @Document LIKE '%[^0-9]%'
        BEGIN
            SET @ErrorMessage = 'Documento deve conter apenas números';
            THROW 50007, @ErrorMessage, 1;
        END
        
        -- ========================================================================
        -- VALIDAÇÕES DE DUPLICIDADE
        -- ========================================================================
        
        -- Verificar duplicidade de CNPJ
        IF @PersonalityType = 'J' AND EXISTS (SELECT 1 FROM [dbo].[partners] WHERE [cnpj] = @Document)
        BEGIN
            SET @ErrorMessage = 'Já existe um parceiro cadastrado com este CNPJ';
            THROW 50008, @ErrorMessage, 1;
        END
        
        -- Verificar duplicidade de CPF
        IF @PersonalityType = 'F' AND EXISTS (SELECT 1 FROM [dbo].[partners] WHERE [cpf] = @Document)
        BEGIN
            SET @ErrorMessage = 'Já existe um parceiro cadastrado com este CPF';
            THROW 50009, @ErrorMessage, 1;
        END
        
        -- Verificar duplicidade de email
        IF EXISTS (SELECT 1 FROM [dbo].[partners] WHERE [email] = @Email)
        BEGIN
            SET @ErrorMessage = 'Já existe um parceiro cadastrado com este email';
            THROW 50010, @ErrorMessage, 1;
        END
        
        -- ========================================================================
        -- INSERÇÃO DOS DADOS
        -- ========================================================================
        
        INSERT INTO [dbo].[partners] (
            [company_name],
            [cnpj],
            [cpf],
            [email],
            [zip_code],
            [state],
            [city],
            [street],
            [number],
            [neighborhood],
            [phone],
            [complement],
            [observations],
            [created_at],
            [updated_at]
        )
        VALUES (
            LTRIM(RTRIM(@CompanyName)),
            CASE WHEN @PersonalityType = 'J' THEN @Document ELSE NULL END,
            CASE WHEN @PersonalityType = 'F' THEN @Document ELSE NULL END,
            LOWER(LTRIM(RTRIM(@Email))),
            @ZipCode,
            UPPER(LTRIM(RTRIM(@State))),
            LTRIM(RTRIM(@City)),
            LTRIM(RTRIM(@Street)),
            LTRIM(RTRIM(@Number)),
            LTRIM(RTRIM(@Neighborhood)),
            LTRIM(RTRIM(@Phone)),
            NULLIF(LTRIM(RTRIM(@Complement)), ''),
            NULLIF(LTRIM(RTRIM(@Observations)), ''),
            GETUTCDATE(),
            GETUTCDATE()
        );
        
        -- Obter ID do parceiro inserido
        SET @PartnerId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
        -- ========================================================================
        -- RETORNO DE SUCESSO
        -- ========================================================================
        
        SELECT 
            @PartnerId as Id,
            'Parceiro cadastrado com sucesso' as Message,
            'SUCCESS' as Status,
            GETUTCDATE() as Timestamp;
            
        -- Retornar dados do parceiro inserido
        SELECT 
            p.[id],
            p.[company_name],
            CASE 
                WHEN p.[cnpj] IS NOT NULL THEN 
                    STUFF(STUFF(STUFF(STUFF(p.[cnpj], 3, 0, '.'), 7, 0, '.'), 11, 0, '/'), 16, 0, '-')
                WHEN p.[cpf] IS NOT NULL THEN 
                    STUFF(STUFF(STUFF(p.[cpf], 4, 0, '.'), 8, 0, '.'), 12, 0, '-')
            END as [formatted_document],
            CASE 
                WHEN p.[cnpj] IS NOT NULL THEN 'Pessoa Jurídica'
                ELSE 'Pessoa Física'
            END as [person_type],
            p.[email],
            p.[zip_code],
            p.[state],
            p.[city],
            p.[street],
            p.[number],
            p.[neighborhood],
            p.[complement],
            p.[phone],
            p.[observations],
            p.[created_at],
            p.[updated_at]
        FROM [dbo].[partners] p
        WHERE p.[id] = @PartnerId;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Retorno de erro
        SELECT 
            ERROR_NUMBER() as ErrorNumber,
            ERROR_MESSAGE() as Message,
            'ERROR' as Status,
            GETUTCDATE() as Timestamp;
            
        -- Re-throw do erro para o chamador
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure sp_inserir_parceiro criada com sucesso!';