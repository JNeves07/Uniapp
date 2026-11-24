# UniApp - Economia Circular e Apoio Acadêmico

Plataforma mobile multiplataforma (**Android e iOS**) e API REST desenvolvida para conectar estudantes universitários, promovendo a economia circular por meio da compra, venda, troca e doação de materiais acadêmicos, além da organização de grupos de estudo colaborativos.

## Visão Geral do Projeto

O ingresso e a permanência no ensino superior envolvem custos expressivos com livros didáticos, apostilas, calculadoras científicas e equipamentos de laboratório. Paralelamente, muitos materiais permanecem ociosos ou são descartados ao término de cada semestre. 

O **UniApp** centraliza a comunidade acadêmica em um ambiente exclusivo (com validação institucional), oferecendo:
- **Marketplace Acadêmico:** Publicação e busca de insumos para venda, troca ou doação, com filtros por curso, categoria e estado de conservação.
- **Apoio Acadêmico:** Criação e participação em grupos de estudo presenciais ou remotos organizados por disciplina e horário.
- **Sistema de Reputação:** Avaliação entre pares para assegurar confiança e transparência nas negociações dentro do campus.

## Arquitetura e Tecnologias Utilizadas

O sistema foi projetado em camadas desacopladas, utilizando tecnologias modernas do ecossistema .NET e conteinerização:

- **Front-end Mobile (Android & iOS):** .NET 8 MAUI (C# / XAML) estruturado no padrão arquitetural MVVM com `CommunityToolkit.Mvvm`.
- **Back-end (API REST):** ASP.NET Core 8 Web API com Entity Framework Core e documentação interativa via Swagger/OpenAPI.
- **Autenticação e Segurança:** JSON Web Tokens (JWT) com restrição de domínio institucional (`.edu.br`).
- **Banco de Dados:** MySQL 8.0.
- **Infraestrutura:** Docker e Docker Compose.

## Estrutura do Repositório

```text
UniApp/
├── docker-compose.yml              # Orquestração do banco de dados MySQL
├── .gitignore                      # Arquivos ignorados pelo controle de versão
├── README.md                       # Documentação do projeto
├── UniApp.sln                      # Solução .NET integrando API e Mobile
├── UniApp.Backend/                 # API REST em ASP.NET Core 8
│   ├── Controllers/                # Endpoints (Auth, Materiais, Grupos)
│   ├── Data/                       # Contexto EF Core e Seed inicial
│   ├── Models/                     # Entidades de domínio
│   ├── Program.cs                  # Configuração de serviços e pipeline HTTP
│   ├── appsettings.json            # Configurações de conexão e JWT
│   └── UniApp.Api.csproj           # Dependências do projeto Back-end
└── UniApp.Mobile/                  # Aplicativo Multiplataforma (Android e iOS)
    ├── Models/                     # DTOs e modelos de exibição
    ├── Platforms/                  # Configurações nativas por sistema operacional
    │   ├── Android/                # AndroidManifest, MainActivity e MainApplication
    │   └── iOS/                    # Info.plist (ATS HTTP local), AppDelegate e Program.cs
    ├── Services/                   # Camada de comunicação HTTP com detecção de plataforma
    ├── ViewModels/                 # Regras de apresentação e comandos MVVM
    ├── Views/                      # Interfaces XAML (Login, Home, Anunciar, Grupos, Perfil)
    ├── App.xaml                    # Dicionário de recursos e paleta de cores
    ├── AppShell.xaml               # Navegação principal por abas
    ├── MauiProgram.cs              # Injeção de dependências
    └── UniApp.Mobile.csproj        # Dependências do projeto Mobile
```

## Pré-requisitos

Para compilar e executar o projeto localmente, certifique-se de ter instalado:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Workload do .NET MAUI (`dotnet workload install maui`)
- [Docker](https://www.docker.com/) e Docker Compose
- Visual Studio 2022 ou Visual Studio Code com C# Dev Kit
- **Para Android:** Emulador Android (API 33 ou superior) ou dispositivo físico via USB
- **Para iOS:** macOS com Xcode instalado (ou Visual Studio no Windows pareado a um Mac build host) e Simulador iOS

## Guia de Instalação e Execução

### 1. Inicializar o Banco de Dados (MySQL)

Na raiz do projeto, suba o container do MySQL utilizando o Docker Compose:

```bash
docker-compose up -d
```

Aguarde alguns segundos até que o serviço esteja ativo na porta `3306`.

### 2. Executar o Back-end (API REST)

Em um terminal, acesse a pasta da API, restaure os pacotes e inicie o servidor:

```bash
cd UniApp.Backend
dotnet restore
dotnet run --urls "http://0.0.0.0:5000"
```

Na primeira execução, o Entity Framework Core criará automaticamente o banco `uniapp_db`, as tabelas e os dados iniciais (Seed). A documentação Swagger estará acessível em `http://localhost:5000/swagger`.

### 3. Executar o Aplicativo Mobile (.NET MAUI)

#### No Android:
```bash
cd UniApp.Mobile
dotnet restore
dotnet build -t:Run -f net8.0-android
```

#### No iOS (macOS / Simulador iOS):
```bash
cd UniApp.Mobile
dotnet restore
dotnet build -t:Run -f net8.0-ios
```

> **Nota sobre rede em Android e iOS:** A classe `ApiService` detecta automaticamente a plataforma em tempo de execução (`DeviceInfo.Platform`). No Emulador Android, as requisições são roteadas para `http://10.0.2.2:5000/api`; no Simulador iOS, são enviadas diretamente para `http://localhost:5000/api` (com permissão de tráfego HTTP local configurada no `Info.plist`).

### Credenciais de Acesso para Testes

O banco de dados é inicializado com contas pré-cadastradas:

- **E-mail Institucional:** `joao.marcelo@unifacema.edu.br`
- **Senha:** `123456`

## Autores

Projeto desenvolvido para a disciplina **Projeto Integrador: Desenvolvimento para Dispositivos Móveis** do curso de Tecnologia em Análise e Desenvolvimento de Sistemas — **UNIFACEMA** (Caxias - MA, 2026).

- **João Marcelo Neves de Lima** - Arquitetura, Desenvolvimento Mobile (.NET MAUI Android/iOS), Back-end (ASP.NET Core API), Banco de Dados (MySQL) e Infraestrutura Docker - [GitHub](https://github.com/JNeves07)
- **Davi dos Santos Monteiro** - Desenvolvimento Front-end, Interface e Prototipagem - [GitHub](https://github.com/DaviDM-1)
- **Italo Santos Moreira de Araújo** - Levantamento de Requisitos e Documentação
- **Orientador:** Prof. Marcos Gomes da Silva Rocha
