# UniApp - Economia Circular e Apoio Acadêmico

Plataforma mobile multiplataforma (**Android, iOS e Windows**) e API REST desenvolvida para conectar estudantes universitários, promovendo a economia circular por meio da compra, venda, troca e doação de materiais acadêmicos, além da organização de grupos de estudo colaborativos.

## Visão Geral do Projeto

O ingresso e a permanência no ensino superior envolvem custos expressivos com livros didáticos, apostilas, calculadoras científicas e equipamentos de laboratório. Paralelamente, muitos materiais permanecem ociosos ou são descartados ao término de cada semestre.

O **UniApp** centraliza a comunidade acadêmica em um ambiente exclusivo (com validação institucional), oferecendo:

- **Marketplace Acadêmico:** publicação e busca de insumos para venda, troca ou doação, com filtros por curso, categoria e estado de conservação.
- **Apoio Acadêmico:** criação e participação em grupos de estudo presenciais ou remotos organizados por disciplina e horário.
- **Segurança:** cadastro restrito a e-mails institucionais `.edu.br`, senhas com hash PBKDF2 e autenticação JWT com access/refresh token.
- **Sistema de Reputação:** avaliação entre pares para assegurar confiança e transparência nas negociações dentro do campus.

## Tecnologias Utilizadas

| Camada | Tecnologia |
| --- | --- |
| Back-end | **.NET 8**, ASP.NET Core Web API, Entity Framework Core 8, Pomelo MySQL |
| Banco de dados | **MySQL 8.0** via Docker (SQLite disponível para testes/demo) |
| Segurança | **JWT** (HMAC-SHA256), PBKDF2-SHA256, CORS explícito, rate limiting, ProblemDetails |
| Validação | FluentValidation + regras de domínio |
| Mobile | **.NET MAUI** (C#/XAML), MVVM com `CommunityToolkit.Mvvm` (alvos `net10.0-*`) |
| Testes | **xUnit**, EF Core InMemory e SQLite, `WebApplicationFactory` |
| Infraestrutura | **Docker** e Docker Compose |
| CI | **GitHub Actions** |
| Documentação da API | Swagger/OpenAPI |

> O back-end e os testes usam `net8.0`. O projeto MAUI já estava configurado com `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0` e foi mantido assim para não quebrar os workloads instalados.

## Arquitetura em Camadas

A API está organizada em camadas dentro de `UniApp.Backend/`, preservando os endpoints existentes:

```text
UniApp/
├── .github/workflows/ci.yml          # CI: build/test do back-end + job MAUI opcional
├── .env.example                      # Modelo de variaveis de ambiente (sem segredos reais)
├── Dockerfile                        # Imagem da API (multi-stage, usuario nao-root)
├── docker-compose.yml                # MySQL + API com healthcheck e variaveis de ambiente
├── dotnet-tools.json                 # Ferramenta local dotnet-ef (migracoes)
├── UniApp.sln
├── UniApp.Backend/                   # API ASP.NET Core 8
│   ├── Api/
│   │   ├── Controllers/              # Auth, Grupos e Materiais (endpoints atuais)
│   │   ├── Middlewares/              # Tratamento global de erros -> ProblemDetails
│   │   └── Program.cs                # DI, JWT, CORS, rate limiting, Swagger, seed
│   ├── Application/
│   │   ├── DTOs/                     # Contratos de entrada/saida
│   │   ├── Interfaces/               # Servicos e repositorios (contratos)
│   │   ├── Mapping/                  # Entidade -> DTO (nunca expoe SenhaHash)
│   │   ├── Services/                 # Casos de uso (auth, grupos, materiais)
│   │   └── Validation/               # Validadores FluentValidation
│   ├── Domain/
│   │   ├── Entities/                 # Usuario, GrupoEstudo, Material
│   │   ├── Exceptions/               # DomainException, NotFoundException, etc.
│   │   ├── Rules/                    # .edu.br, vagas, modalidades, precos
│   │   └── Services/                 # Contrato do hasher de senha
│   ├── Infrastructure/
│   │   ├── Data/                     # AppDbContext, migracoes e seed idempotente
│   │   ├── Migrations/               # Migracao EF Core inicial (MySQL)
│   │   ├── Repositories/             # Implementacoes EF Core
│   │   └── Security/                 # JWT e PBKDF2
│   ├── appsettings.json              # Configuracao sem segredos
│   └── appsettings.Development.json  # Valores de EXEMPLO para desenvolvimento
├── UniApp.Mobile/                    # App .NET MAUI (MVVM)
│   ├── Models/, Services/, ViewModels/, Views/, Platforms/
│   └── MauiProgram.cs
└── tests/UniApp.Backend.Tests/        # xUnit (dominio, servicos, endpoints e seed)
```

Fluxo de dependência: `Api -> Application -> Domain`, e `Infrastructure -> Application/Domain`. A `Api` compõe a implementação concreta via injeção de dependências; entidades de domínio não conhecem EF Core.

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (back-end e testes)
- [.NET 10 SDK + workload MAUI](https://dotnet.microsoft.com/download/dotnet/10.0) para o app (`dotnet workload install maui`)
- [Docker](https://www.docker.com/) com Docker Compose
- Visual Studio 2022/2026 ou VS Code com C# Dev Kit
- **Android:** emulador (API 21+) ou dispositivo físico via USB
- **iOS:** macOS com Xcode (ou build host Mac pareado ao Windows)

## Configuração de segredos

Nenhum segredo é versionado. `appsettings.json` contém apenas valores neutros; `appsettings.Development.json` traz **valores de exemplo** para rodar localmente.

Em produção, sobrescreva por variáveis de ambiente ou user-secrets:

```bash
# user-secrets (desenvolvimento)
dotnet user-secrets set "Jwt:Key" "sua-chave-aleatoria-com-mais-de-32-caracteres" --project UniApp.Backend/UniApp.Api.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=uniapp_db;Uid=root;Pwd=SUA_SENHA;" --project UniApp.Backend/UniApp.Api.csproj

# variáveis de ambiente (Docker/CI)
Jwt__Key=...
ConnectionStrings__DefaultConnection=...
```

A API falha na inicialização se `Jwt:Key` estiver vazia ou com menos de 32 bytes.

## Guia de Instalação e Execução

### 1. Banco de dados (MySQL com Docker)

```bash
cp .env.example .env
# Edite .env e troque MYSQL_ROOT_PASSWORD e JWT_KEY
docker compose up -d mysql
```

O serviço `mysql` possui healthcheck; a API só inicia quando o banco estiver pronto. Porta padrão: `3306`.

Para subir MySQL **e** API juntos:

```bash
docker compose up -d --build
```

A API fica em `http://localhost:5000` (Use `ASPNETCORE_ENVIRONMENT=Development` no `.env` para habilitar Swagger no container).

### 2. Executar a API localmente

```bash
cd UniApp.Backend
dotnet restore
dotnet run
```

Na primeira execução, a aplicação aplica a migração EF Core (`Database.Migrate()`), cria as tabelas e roda o seed idempotente (3 usuários, 3 materiais e 3 grupos). Swagger: `http://localhost:5000/swagger`.

Para usar o provider SQLite (sem MySQL) em desenvolvimento/teste:

```bash
dotnet run -- --Database:Provider=Sqlite --ConnectionStrings:DefaultConnection="Data Source=uniapp-dev.db"
```

#### Migrações

```bash
dotnet tool restore
dotnet dotnet-ef migrations add NomeDaMigracao \
  --project UniApp.Backend/UniApp.Api.csproj \
  --startup-project UniApp.Backend/UniApp.Api.csproj \
  --output-dir Infrastructure/Migrations
dotnet dotnet-ef database update --project UniApp.Backend/UniApp.Api.csproj --startup-project UniApp.Backend/UniApp.Api.csproj
```

### 3. Executar o aplicativo MAUI

```bash
cd UniApp.Mobile
dotnet restore

# Android (emulador)
dotnet build -t:Run -f net10.0-android

# iOS (macOS com Xcode)
dotnet build -t:Run -f net10.0-ios

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

Por padrão, `ApiService` usa `http://10.0.2.2:5000/api/` no emulador Android e `http://localhost:5000/api/` no simulador iOS/Windows. Para um dispositivo físico na mesma rede, configure em tempo de execução:

```csharp
ApiService.DefinirBaseUrl("http://192.168.1.10:5000/api/");
```

O app armazena access/refresh token com `SecureStorage`, renova automaticamente o access token após 401, aplica timeout de 15 s e exibe mensagens amigáveis (`ApiService.UltimoErro`). Quando a API está indisponível, mantém o modo demonstração com dados de exemplo e avisa o usuário.

### Credenciais de demonstração

- **E-mail institucional:** `joao.marcelo@unifacema.edu.br`
- **Senha:** `123456`

> O seed grava senhas com PBKDF2 (salt aleatório), nunca em texto claro.

## Endpoints da API

| Método | Rota | Descrição | Autenticação |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | Login institucional; retorna `token`, `refreshToken` e `usuario` | Não |
| `POST` | `/api/auth/registrar` | Cadastro com e-mail `.edu.br` | Não |
| `POST` | `/api/auth/refresh` | Emite novo access token a partir do refresh token | Não (usa refresh token) |
| `GET` | `/api/auth/me` | Dados do usuário do token (exemplo de endpoint protegido) | Bearer |
| `GET` | `/api/grupos?formato=&busca=` | Lista grupos de estudo | Não |
| `POST` | `/api/grupos` | Cria grupo de estudo | Não |
| `POST` | `/api/grupos/{id}/participar` | Reserva uma vaga no grupo | Não |
| `GET` | `/api/materiais?categoria=&busca=` | Lista anúncios | Não |
| `POST` | `/api/materiais` | Publica anúncio | Não |
| `GET` | `/health` | Health check da API | Não |

Os endpoints públicos foram mantidos compatíveis com o cliente MAUI existente. Erros são devolvidos como `ProblemDetails` (com a extensão `mensagem` para compatibilidade): 400 para regras de negócio/validação, 401 para credenciais inválidas, 404 para recurso inexistente e 500 para erros inesperados.

**Segurança adicional:** JWT valida issuer (`UniAppApi`), audience (`UniAppMobile`), assinatura e expiração (access 60 min, refresh 30 dias). CORS aceita apenas as origens configuradas em `Cors:AllowedOrigins`. Rate limiting: 100 req/min por IP (global) e 10 req/min por IP nos endpoints de autenticação (`429`).

## Testes

```bash
dotnet test tests/UniApp.Backend.Tests/UniApp.Backend.Tests.csproj
```

A suíte cobre:

- regras institucionais `.edu.br`, vagas de grupo, modalidades/preço de materiais e hash de senha;
- login, cadastro, duplicidade, refresh token (issuer/audience/expiração);
- criação/listagem de grupos e materiais com EF Core InMemory;
- seed idempotente com SQLite;
- integração HTTP dos endpoints com `WebApplicationFactory` e SQLite.

## CI (GitHub Actions)

`.github/workflows/ci.yml` executa, em cada push/PR para `main`:

1. `dotnet restore` do back-end e dos testes;
2. build em `Release`;
3. `dotnet test` com publicação do relatório TRX como artifact.

O job do MAUI é separado, **opcional** (somente `workflow_dispatch`) e `continue-on-error: true`, pois exige a instalação de workloads pesados e não deve bloquear o CI.

## Autores

Projeto desenvolvido para a disciplina **Projeto Integrador: Desenvolvimento para Dispositivos Móveis** do curso de Tecnologia em Análise e Desenvolvimento de Sistemas — **UNIFACEMA** (Caxias - MA, 2026).

- **João Marcelo Neves de Lima** - Arquitetura, Desenvolvimento Mobile (.NET MAUI Android/iOS), Back-end (ASP.NET Core API), Banco de Dados (MySQL) e Infraestrutura Docker - [GitHub](https://github.com/JNeves07)
- **Davi dos Santos Monteiro** - Desenvolvimento Front-end, Interface e Prototipagem - [GitHub](https://github.com/DaviDM-1)
- **Italo Santos Moreira de Araújo** - Levantamento de Requisitos e Documentação
- **Orientador:** Prof. Marcos Gomes da Silva Rocha
