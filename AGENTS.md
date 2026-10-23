# AGENTS.md: instruções para IAs de código (zum-commerce-api)

API em C# da plataforma **Zum Commerce**: SaaS multi-loja de e-commerce sobre Shopify, da Zum Web Studio.
Antes de qualquer tarefa grande, leia **`docs/ESCOPO_DO_PROJETO.md`** (produto, dinâmicas de compra, pendências por prioridade). A arquitetura está em **`ARCHITECTURE.md`**, e as rotas em **`backend/docs/ENDPOINTS.md`**.

## Repositórios

| Repositório | Conteúdo |
|---|---|
| `github.com/luizsalves/zum-commerce-api` (este) | API (.NET 10) |
| `github.com/luizsalves/zum-commerce` | Front: painel + vitrine padrão (React), regras do Firebase |

Não faça `push`, não crie branches remotas e não abra pull requests sem pedido explícito do usuário.

Localmente, os dois ficam lado a lado (`C:\Projetos\Zum\zum-commerce` e `C:\Projetos\Zum\zum-commerce-api`). O `docs/ESCOPO_DO_PROJETO.md` é cópia do arquivo do repositório do front, que é a fonte oficial. Ao alterar um, atualize o outro.

## Comandos (rodar em `backend/`)

```bash
dotnet build ZumCommerce.Backend.slnx
dotnet test ZumCommerce.Backend.slnx
dotnet run --project src/Core.Web          # http://localhost:8080
```

Configuração local: `src/Core.Web/appsettings.Development.json`, criado a partir do `.example` e **nunca commitado**.

## Estrutura e regras de dependência

```
backend/src/
├── Core.Domain/                      modelos e constantes, sem depender de nada
├── Core.Application/                 contratos, interfaces, TenantAccessService, casos de uso
├── Core.Infrastructure/              Firestore, Shopify (GraphQL), AES-GCM, Firebase Auth
├── Core.Infrastructure.CrossCutting/ utilitários genéricos (telefone, dinheiro, strings)
├── Core.Infrastructure.IoC/          AddCoreServices(): registra tudo
└── Core.Web/                         Program.cs, controllers, autenticação, middleware de erros
backend/tests/Core.Web.Tests/         xUnit
```

- `Core.Domain` não referencia nenhum outro projeto.
- `Core.Application` **não** usa Firestore, Shopify, Firebase nem ASP.NET. Tudo passa por interfaces em `Application/Interfaces`.
- `Core.Infrastructure` implementa as interfaces. Cada integração externa nova (ERP, frete, gateway) ganha uma pasta própria aqui.
- `Core.Web` só recebe a requisição e chama o caso de uso. Controllers não têm regra de negócio.
- Todo caso de uso novo deve ser registrado em `Core.Infrastructure.IoC/DependencyInjection.cs`.

## Regras obrigatórias

1. **Segredos:** Client Secret e tokens Admin da Shopify só ficam em `tenantSecrets/{id}`, criptografados com `ISecretProtector`. Nunca em log, resposta HTTP, `tenants/{id}` ou código.
2. **Multi-loja:** toda rota de loja recebe `X-Tenant-Id` e valida com `TenantAccessService` (`RequireMemberAsync` no painel, `RequireTenantAsync` na vitrine, `RequirePlatformAdmin` na área da Zum). Nunca confie em ID de loja vindo do corpo da requisição.
3. **Erros:** use `AppException` (Validation, Forbidden, NotFound, Unprocessable, External…), com mensagem **em português** e entendível pelo lojista. O middleware converte em status HTTP.
4. **Shopify:**
   - Use `ShopifyGraphQLClient` (cuida de token, retentativa e limite de taxa) e chame `ThrowOnUserErrors` em toda mutation.
   - Queries e mutations ficam em `ShopifyQueries.cs`.
   - Antes de usar uma mutation ou campo novo, **confira na documentação da Shopify** da versão em uso (`2026-10`).
   - `productSet` trata listas como completas: reenvie fotos e variações existentes por `id`.
5. **Testes:** toda regra nova (validação, cálculo, acesso) tem teste em `Core.Web.Tests`. Use os fakes de `Fakes/`, sem banco e sem rede.
6. **Estilo:** `Nullable` ligado, `ImplicitUsings`, namespaces por pasta, versões de pacote só em `Directory.Packages.props`.

## Como trabalhar

- Antes de dizer que terminou: `dotnet build` e `dotnet test` sem erros. Se não conseguir rodar, diga isso claramente.
- Mudanças grandes: apresente o plano (arquivos novos e alterados) antes de executar.
- Não apague nem renomeie arquivos sem necessidade explícita.
- Não adicione pacotes (MediatR, AutoMapper, repositório genérico etc.) sem pedir.
- Ao terminar uma pendência do escopo, atualize a seção "Status" do `ESCOPO_DO_PROJETO.md` nos dois repositórios.

## Git

- Branch principal: `main`. Trabalho em branches `feature/<assunto>` ou `fix/<assunto>`.
- Mensagens de commit em português, no imperativo e curtas (ex.: `Publica produto no canal da loja após salvar`).
- Commit e push só quando o usuário pedir.

## Situação atual (atualize quando mudar)

- O código **ainda não foi compilado**: a primeira tarefa é `dotnet build`, `dotnet test` e corrigir o que aparecer.
- **Bug P0:** o produto salvo não é publicado no canal de vendas, por isso não aparece no site. Corrigir com `publishablePublish` e `publishableUnpublish` (ver escopo, item 5.3).
