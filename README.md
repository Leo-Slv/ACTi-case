# ACTi-case — Full setup guide

Este README explica passo a passo como clonar e rodar o projeto **ACTi-case** (backend .NET, banco SQL Server e frontend Angular). Use este documento como checklist local — ele assume Windows como SO de desenvolvimento (pode ser adaptado para WSL/Linux/macOS).

> Repositório: [https://github.com/Leo-Slv/ACTi-case](https://github.com/Leo-Slv/ACTi-case). (Conteúdo do repositório referenciado neste guia.)

---

## Sumário

1. Pré-requisitos
2. Clonar o repositório
3. Banco de dados (SQL Server)
4. Backend (.NET)
5. Frontend (Angular)
6. Problemas comuns e debugging
7. Contato / referências

---

## 1) Pré-requisitos

Instale as ferramentas abaixo antes de começar:

* .NET SDK (recomendo a versão mais recente compatível com o projeto; instalar a partir do site oficial). Veja downloads oficiais do .NET: [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download).
* Node.js (LTS) e npm — necessário para o frontend Angular. Página oficial: [https://nodejs.org/en/download/](https://nodejs.org/en/download/).
* Angular CLI (opcional se o frontend já estiver no repositório, mas útil para rodar localmente ou gerar um novo frontend): `npm install -g @angular/cli`.
* Microsoft SQL Server Developer/Express (para desenvolvimento local). Baixe no site da Microsoft (SQL Server Developer/Express).
* SQL Server Management Studio (SSMS) ou `sqlcmd` para executar scripts e inspecionar o banco (opcional, mas recomendado).

Referências oficiais de instalação (links de referência estão no final do documento).

---

## 2) Clonar o repositório

No terminal/clá!sh do seu ambiente de desenvolvimento, rode:

```bash
git clone https://github.com/Leo-Slv/ACTi-case.git
cd ACTi-case
```

A estrutura principal do repositório contém (resumo):

* `backend/` — código do backend (.NET)
* `banco/` — scripts SQL e configuração do banco
* possivelmente um diretório `frontend/` (se houver, contém o app Angular). Caso o frontend não esteja no repositório, as instruções abaixo explicam como criar/rodar um frontend local.

---

## 3) Banco de dados (SQL Server)

### 3.1 Criar a base local

1. Abra o SQL Server Management Studio (ou use `sqlcmd`).
2. Crie o banco conforme o script do repositório. Dentro da pasta `banco/` deve haver scripts para criar o banco e as tabelas (procure por algo como `create_database.sql` ou `script.sql`). Se houver um script principal, execute-o.

Exemplo rápido (executar no SSMS - nova query):

```sql
-- cria um banco de exemplo chamado ACTiDb
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ACTiDb')
BEGIN
    CREATE DATABASE ACTiDb;
END
GO
USE ACTiDb;
GO
-- depois rode os scripts de criação de tabelas / inserts que estiverem em `banco/`
```

> Obs: O repositório já inclui scripts na pasta `banco/`. Rode-os na ordem indicada pelos nomes (se houver prefixos numéricos) para garantir a criação de estruturas e dados.

### 3.2 Connection string de exemplo

No ambiente de desenvolvimento Windows com SQL Server local (autenticação Windows) você pode usar algo como:

```
Server=localhost;Database=ACTiDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

Se usar SQL Authentication (usuário/senha):

```
Server=localhost;Database=ACTiDb;User Id=sa;Password=SuaSenhaAqui;MultipleActiveResultSets=true
```

Guarde essa connection string — o backend precisa dela (veja seção Backend).

---

## 4) Backend (.NET)

> Observação: no repositório o projeto de API costuma estar em `backend/src/ACTi.API` (por exemplo `dotnet run --project src/ACTi.API`). Ajuste caminhos caso estejam diferentes.

### 4.1 Configurar variáveis e connection string

1. Abra o projeto no Visual Studio ou edite os arquivos `appsettings.Development.json` / `appsettings.json` no projeto `ACTi.API` dentro de `backend/src/ACTi.API`. Localize a chave `ConnectionStrings` e atualize com a connection string para o seu banco local (ex.: `ACTiDb`).

Exemplo `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ACTiDb;User Id=sa;Password=SuaSenhaAqui;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

> Se o projeto usa variáveis de ambiente (ex.: `ASPNETCORE_ConnectionStrings__DefaultConnection`), prefira definir via variáveis de ambiente em vez de editar arquivos, especialmente em ambientes públicos.

### 4.2 Restaurar dependências, build e rodar

No terminal, a partir da pasta `backend` (ou `backend/src/ACTi.API`) rode:

```bash
# ir para a pasta do backend
cd backend
# restaurar pacotes
dotnet restore
# compilar
dotnet build
# rodar (ajuste o caminho do projeto se necessário)
dotnet run --project src/ACTi.API
```

Se o projeto estiver configurado para `launchSettings.json` com profiles, você também pode abrir com Visual Studio e executar via IIS Express / Kestrel.

### 4.3 Endpoints e testes

* Após rodar, verifique o console para a URL (ex.: `http://localhost:5000` ou `https://localhost:7001`).
* Teste com `curl`, Postman, Insomnia ou via browser os endpoints (ex.: `GET /api/health` ou `GET /swagger` caso a API exponha Swagger).

---

## 5) Frontend (Angular)

> Nem sempre o repositório contém um frontend pronto — se não houver, as instruções abaixo mostram como criar/rodar um frontend Angular que consome a API.

### 5.1 Se houver pasta `frontend` no repositório

1. Entre na pasta do frontend: `cd frontend` (ou o nome que existir).
2. Instale dependências: `npm install`
3. Abra `src/environments/environment.ts` e configure a URL base da API apontando para o backend local (ex.: `http://localhost:5000/api`).
4. Rode o frontend: `ng serve` (ou `npm start` se já existir script).
5. Acesse `http://localhost:4200` no browser.

### 5.2 Se NÃO houver frontend e você quer criar um rápido para testes

```bash
# instale angular cli se necessário
npm install -g @angular/cli
# crie uma nova app (dentro do diretório do repo para versionamento opcional)
ng new frontend --routing --style=scss
cd frontend
# ajustar proxy (opcional) ou environment.ts para apontar a API
ng serve
```

Para desenvolvimento, criar um `proxy.conf.json` ajuda a redirecionar chamadas `/api` para o backend sem lidar com CORS:

`proxy.conf.json`:

```json
{
  "/api": {
    "target": "http://localhost:5000",
    "secure": false,
    "changeOrigin": true
  }
}
```

E rodar: `ng serve --proxy-config proxy.conf.json`.

---

## 6) Problemas comuns & dicas de debugging

* **Erro ao conectar no banco**: verifique se o SQL Server está rodando, se a base `ACTiDb` existe e se a connection string (usuário/senha) está correta. Teste a conexão com SSMS.
* **`dotnet run` falhando por DI (Cannot instantiate implementation type ...)**: isso normalmente indica que você registrou uma interface no `ConfigureServices` apontando para uma interface em vez de uma classe concreta, ou injetou incorretamente o tipo. Verifique `Startup` / `Program.cs` e as linhas de `services.AddScoped<IFoo, Foo>();`.
* **CORS no frontend**: se o frontend não conseguir chamar a API, habilite CORS no backend (`builder.Services.AddCors(...)`) ou use o proxy do Angular.
* **Versões mismatched**: confirme a versão do .NET SDK instalada com `dotnet --info` e a versão do Node com `node -v`.

---

## 7) Contato e referências

* Repositório: [https://github.com/Leo-Slv/ACTi-case](https://github.com/Leo-Slv/ACTi-case).
* .NET downloads: [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download).
* Node.js downloads: [https://nodejs.org/en/download/](https://nodejs.org/en/download/).
* SQL Server downloads: [https://www.microsoft.com/en-us/sql-server/sql-server-downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).

---

## Anotações finais

* Este guia foi criado para dar um passo-a-passo prático e rápido para rodar o projeto localmente. Se preferir, posso gerar um arquivo `appsettings.Development.json` de exemplo com a connection string preenchida (não incluir senhas reais em repositórios públicos).

---

*Gerado por assistente — se quiser, adapto o README para detalhes específicos do repositório (por ex.: nomes exatos dos projetos dentro de `backend/src`) se você me pedir para abrir arquivos específicos.*
