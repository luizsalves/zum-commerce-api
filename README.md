# Zum Commerce: API (back-end)

API em C# da plataforma Zum Commerce. Ela guarda as credenciais da Shopify de cada loja (criptografadas), valida o login do Firebase e faz tudo que exige a Admin API: produtos, estoque, fotos e clientes. O front fica no repositório separado **`zum-commerce`**.

| Item | Valor |
|---|---|
| Stack | ASP.NET Core Web API, .NET 10 (`global.json`: SDK 10.0.100) |
| Banco | Firestore |
| Autenticação | Firebase Auth (JWT Bearer) |
| Integrações | Shopify Admin API (GraphQL), Firebase Admin |
| Deploy | Google Cloud Run (`backend/Dockerfile`) |

## Estrutura

```text
zum-commerce-api/
├── README.md, ARCHITECTURE.md
├── docs/CONFIGURACAO.md          Firebase, Shopify, segredos e deploy passo a passo
└── backend/
    ├── ZumCommerce.Backend.slnx
    ├── global.json, Directory.Build.props, Directory.Packages.props
    ├── Dockerfile, .dockerignore
    ├── docs/ENDPOINTS.md
    ├── src/
    │   ├── Core.Domain/                     modelos: loja, credenciais, membro, produto, cliente
    │   ├── Core.Application/                contratos, interfaces, regras de acesso e casos de uso
    │   ├── Core.Infrastructure/             Firestore, Shopify, criptografia, Firebase Auth
    │   ├── Core.Infrastructure.CrossCutting/ utilitários (telefone E.164, dinheiro, strings)
    │   ├── Core.Infrastructure.IoC/         registro das dependências
    │   └── Core.Web/                        host, controllers, autenticação, tratamento de erros
    └── tests/Core.Web.Tests/                xUnit
```

Detalhes em [ARCHITECTURE.md](ARCHITECTURE.md).

## Rodar local

Pré-requisitos: .NET 10 SDK e Google Cloud CLI (`gcloud`).

```bash
cd backend/src/Core.Web
cp appsettings.Development.example.json appsettings.Development.json   # preencha
openssl rand -base64 32          # cole o resultado em Secrets:Key
gcloud auth application-default login
dotnet run                       # http://localhost:8080
```

Ou use o `iniciar-backend.bat` do repositório `zum-commerce`.

| Comando (em `backend/`) | O quê |
|---|---|
| `dotnet build ZumCommerce.Backend.slnx` | Compila a solução |
| `dotnet test ZumCommerce.Backend.slnx` | Roda os testes |
| `dotnet run --project src/Core.Web` | Sobe a API |

Em desenvolvimento, `GET /openapi/v1.json` descreve a API e `GET /health` responde se o processo está no ar.

## Configuração

| Chave (env no Cloud Run) | Para quê |
|---|---|
| `Firebase:ProjectId` (`Firebase__ProjectId`) | Projeto Firebase: validação do token e Firestore |
| `Platform:AdminEmails` (`Platform__AdminEmails`) | E-mails da equipe Zum, separados por vírgula |
| `Cors:Origins` (`Cors__Origins`) | Origens do front permitidas |
| `Secrets:Key` (`Secrets__Key`) | Chave AES-256 (32 bytes em base64) para criptografar credenciais. Use o Secret Manager. |

Passo a passo completo (Firebase, Shopify, Cloud Run): [docs/CONFIGURACAO.md](docs/CONFIGURACAO.md).

Instruções para IAs de código: [AGENTS.md](AGENTS.md). Escopo completo: [docs/ESCOPO_DO_PROJETO.md](docs/ESCOPO_DO_PROJETO.md).

Repositórios: [luizsalves/zum-commerce-api](https://github.com/luizsalves/zum-commerce-api) (este) e [luizsalves/zum-commerce](https://github.com/luizsalves/zum-commerce).
