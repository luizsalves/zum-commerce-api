# Escopo — Zum Commerce (SaaS de e-commerce multi-loja sobre Shopify)

> **Para a IA que for trabalhar neste projeto:** este documento descreve o produto, a arquitetura, todas as dinâmicas de compra/carrinho/cadastro, o que já foi implementado e o que falta. Siga as regras da seção 9. Quando algo aqui conflitar com o código, avise antes de mudar. Itens marcados **[confirmar]** dependem da documentação atual da Shopify e devem ser verificados antes de implementar.

---

## 1. Objetivo

Plataforma SaaS da **Zum Web Studio** para lojas que vendem pela **Shopify**:

- A Zum administra **todas as lojas dos clientes num único painel**.
- Cada lojista tem **sua loja dentro do SaaS** (seus produtos, clientes, taxas, acessos), isolada das demais.
- Cada loja vende por um **site**: um site **sob medida** (ex.: Elika Tiaras) ou a **vitrine padrão** do SaaS.
- O lojista **cadastra produtos no painel do SaaS** (não no admin da Shopify), com **simulador de taxas** mostrando quanto recebe líquido.
- O cliente final **só compra com cadastro** (login Google, Facebook ou e-mail).
- **Pagamento e checkout são sempre da Shopify** (gateway Mercado Pago ou outro).

Modelo de negócio: implantação + mensalidade por loja.

---

## 2. Peças do sistema

```
┌────────────────────────── SaaS Zum Commerce ──────────────────────────┐
│  zum-commerce (front)                 zum-commerce-api (back, C#)     │
│  • Painel /admin (lojista e Zum)      • Credenciais Shopify por loja  │
│  • Vitrine padrão                     • Shopify Admin API             │
│  Firebase Hosting                     • Cadastro de clientes          │
│                                       Cloud Run                       │
└───────────────────────────────────────────────────────────────────────┘
          ▲                     ▲
          │ login/cadastro      │ (só no login, cadastro, finalizar compra, minha conta)
          │                     │
┌─────────┴─────────┐   ┌───────┴──────────┐        ┌──────────────────┐
│ Site sob medida   │   │ Vitrine padrão   │──────► │ Shopify          │
│ (ex.: elika)      │──►│ (dentro do SaaS) │ produtos│ Storefront API  │
│ projeto separado  │   └──────────────────┘ carrinho│ Checkout        │
└───────────────────┘───────────────────────────────►│ Admin API       │
                                                      └──────────────────┘
Firebase: Auth (login), Firestore (lojas, membros, cadastros, taxas), Storage (fotos)
```

| Peça | Repositório | Stack | Deploy |
|---|---|---|---|
| Front do SaaS (painel + vitrine padrão) | `zum-commerce` | React 19, TypeScript, Vite, Tailwind 4, Firebase SDK | Firebase Hosting |
| API do SaaS | `zum-commerce-api` | ASP.NET Core .NET 10, Firestore, Firebase Admin | Google Cloud Run (`southamerica-east1`) |
| Site sob medida de cada cliente | um repositório por cliente (ex.: `elika-tiaras`, em `C:\Projetos\Loja Virtual\elika-tiaras`) | React + TS + Tailwind | Hosting próprio (Firebase site separado, Vercel etc.) |

**Uma plataforma (API + painel), vários sites.** Com 100 lojas continua sendo 1 API e 1 painel; os sites sob medida são projetos separados. Atenção: Firebase Hosting aceita no máximo **36 sites por projeto**, por isso sites sob medida não devem ficar todos no mesmo projeto Firebase.

---

## 3. Multi-tenant

- **Tenant = loja de um cliente** (ex.: `elika`). ID: letras minúsculas, números e hífen.
- Toda requisição de loja envia o cabeçalho **`X-Tenant-Id`**.
- A API confere: a loja existe e está ativa; o usuário é **membro** da loja (`tenants/{id}/members/{uid}`) ou é da **equipe da plataforma** (e-mails em `Platform:AdminEmails`, com e-mail verificado).
- **Cada loja tem sua própria conexão com a Shopify**:
  - Token **Admin API** (segredo) → guardado **criptografado (AES-256-GCM)** em `tenantSecrets/{id}`, nunca acessível pelo navegador.
  - Token **Storefront API** (público) → em `tenants/{id}`, usado pelo site para produtos e carrinho.
- A vitrine padrão descobre a loja pelo **domínio** (`domains/{host}` → `tenantId`). Site sob medida usa o ID fixo da sua loja.

### Dados no Firestore

| Caminho | Conteúdo | Acesso |
|---|---|---|
| `tenants/{id}` | nome, shopDomain, storefrontToken, apiVersion, cor, logo, WhatsApp, domínios, ativo | leitura pública; membro edita nome/cor/logo/WhatsApp |
| `tenants/{id}/members/{uid}` | e-mail, papel (owner/admin) | só backend grava |
| `tenants/{id}/customers/{uid}` | cadastro do cliente final ↔ Customer da Shopify | cliente lê o próprio; membro lê todos; só backend grava |
| `tenants/{id}/private/fees` | taxas do simulador | membros da loja |
| `tenantSecrets/{id}` | modo, clientId, segredos criptografados | **bloqueado** para o front |
| `users/{uid}.tenants` | índice das lojas que a pessoa administra | dono lê |
| `domains/{host}` | domínio → loja | leitura pública |

### Conexão de cada loja com a Shopify

- **Hoje (manual):** app criado no **Dev Dashboard** da organização da loja → Client ID + Secret → cadastro no painel (aba Plataforma). Token obtido por *client credentials* (dura ~24h, renovado automaticamente com cache). Só funciona quando app e loja estão na **mesma organização**.
- **Alvo (SaaS de verdade):** **um app da Zum** instalado em cada loja via **OAuth** — botão "Conectar minha Shopify" no painel → Shopify pede autorização → API recebe e guarda o token da loja (modo `access_token`, já suportado na API). Webhook `app/uninstalled` desativa a loja. **[confirmar]** instalar o mesmo app em lojas de donos diferentes exige **app público** (pode ser não listado) com **revisão da Shopify**; distribuição personalizada vale só para uma loja/organização Plus.

---

## 4. Dinâmicas do cliente final (site)

### 4.1 Página inicial (vitrine / chamariz)

- Mostra **só alguns produtos**, escolhidos pelo lojista, via **coleções da Shopify**: "Destaques", "Mais vendidos", "Novidades".
- O site consulta a coleção direto na **Storefront API**: `collection(handle: "destaques") { products(first: 8) … }`.
- "Novidades" pode ser automática (produtos ordenados pelos mais recentes).
- Hoje o lojista monta coleções no admin da Shopify. **Pendente:** botão "Destacar na página inicial" no painel do SaaS (API adiciona/remove o produto da coleção pela Admin API).

### 4.2 Catálogo, busca e produto

- Catálogo e busca: todos os produtos **ativos e publicados no canal do site**, direto da Storefront API.
- Página do produto `/produto/{handle}`: fotos, preço, preço "de" riscado, opções (tamanho, cor…), indicação de esgotado por variação.

### 4.3 Carrinho

- O carrinho **vive na Shopify**, não no site.
- No **1º produto adicionado**, o site chama `cartCreate` e recebe um **cart id**; guarda **só o id** no `localStorage` (chave por loja: `cart:{tenantId}`).
- Do 2º produto em diante: `cartLinesAdd`; alterar quantidade: `cartLinesUpdate`; remover: `cartLinesRemove`.
- Se o cliente fechar e voltar depois, o carrinho é recuperado pelo id. Se o carrinho expirou ou virou pedido, o site começa um novo.
- **Adicionar ao carrinho não chama a API do SaaS.**

### 4.4 Login e cadastro obrigatório

- Qualquer pessoa navega e monta carrinho sem login.
- Para **comprar**, precisa de conta: **Google, Facebook ou e-mail/senha** (Firebase Auth). *Login com Instagram não existe para sites de terceiros — fora de escopo.*
- Primeiro acesso numa loja: tela **"Complete seu cadastro"** (nome, sobrenome, celular, aceite de marketing) → API cria o **Customer na Shopify** (ou reaproveita se o e-mail já existir) e grava o vínculo em `tenants/{id}/customers/{uid}`.
- Telefone convertido para E.164 (`+55…`).

### 4.5 Finalizar compra (passagem do bastão)

```
Clique "Finalizar compra"
  ├─ não logado ............... → /entrar (login) → volta
  ├─ logado sem cadastro ...... → /cadastro → API cria Customer na Shopify → volta
  └─ logado com cadastro
        Site → Shopify: cartBuyerIdentityUpdate (e-mail/telefone no carrinho)
        Site → redireciona para cart.checkoutUrl
Checkout Shopify: endereço, frete, pagamento (Mercado Pago), confirmação, e-mail do pedido
```

A **API não recebe o carrinho**; ela só confere/cadastra **quem** compra. O bastão passa do **site para o checkout da Shopify**.

### 4.6 Quais cliques chamam a API do SaaS

| Clique | API do SaaS? | Detalhe |
|---|---|---|
| Abrir página inicial, ver produto, buscar | Não | Storefront API |
| Adicionar ao carrinho / mudar quantidade / remover | Não | Storefront API |
| Entrar (login) | Sim, logo após | `GET /api/store/customers/me` (já tem cadastro nesta loja?) |
| Concluir cadastro | Sim | `POST /api/store/customers/register` |
| Finalizar compra | Sim, se o cadastro ainda não foi confirmado | depois vai para o checkout Shopify |
| Minha conta | Sim | `GET /api/store/customers/me` |
| Meus pedidos | Sim | **pendente** — `GET /api/store/orders/me` |

Motivo: vitrine rápida e barata (não depende do servidor da Zum) e segura (criar cliente exige credencial secreta).

### 4.7 Meus pedidos (pendente)

- Opção escolhida: rota na API `GET /api/store/orders/me` que busca na Admin API os pedidos do Customer vinculado e o site exibe com o visual da loja (status, itens, valor, rastreio).
- Alternativa descartada por ora: link para a conta de cliente da própria Shopify (sai do visual do site).

---

## 5. Dinâmicas do lojista (painel `/admin`)

### 5.1 Acesso

- Login no painel com Google ou e-mail. `GET /api/me` devolve se é equipe da plataforma e quais lojas administra.
- Seletor de loja no topo (quem administra mais de uma).

### 5.2 Cadastro de produtos

- Campos: nome, descrição, categoria (productType), marca (vendor), tags, status (Ativo / Rascunho / Arquivado), fotos, opções (até 3, ex.: Tamanho P/M/G, Cor), e por variação: **preço, preço "de", custo, estoque, peso (g), SKU**.
- Grade de variações gerada automaticamente (Tamanho × Cor), preservando o que já foi digitado; variação nova herda preço/custo/peso; "aplicar em todas".
- Fotos: upload para **Firebase Storage** (`tenants/{id}/products/…`) → URL enviada à Shopify. Primeira foto = capa; reordenar e remover.
- Salvar: `POST /api/admin/product` → API valida → **Shopify `productSet`** (cria/atualiza produto, opções, variações, preços, custo, peso, estoque e fotos numa chamada) → devolve o produto como ficou.
- Produto simples (sem variação): a Shopify exige a opção `Title = "Default Title"`; a API faz isso sozinha.
- `productSet` trata listas como **completas**: a API reenvia fotos existentes por `id` (senão seriam apagadas).

### 5.3 Como o produto novo aparece no site

- A API **não envia nada para o site**. Painel → API → **Shopify**; o site lê da Shopify toda vez que alguém abre a página. Não há cópia nem sincronização.
- Aparece no catálogo/busca se estiver **ativo e publicado no canal de vendas** do site; aparece na página inicial se estiver na coleção de destaque.
- ⚠️ **BUG A CORRIGIR (P0):** produto criado pela Admin API fica ativo, mas **não é publicado em nenhum canal**. A Storefront API só retorna produtos publicados no canal do token (Headless). Correção: após salvar, chamar `publishablePublish` com o `publicationId` do canal da loja (guardar no cadastro da loja); ao mudar para rascunho/arquivado, `publishableUnpublish`. **[confirmar]** nomes/campos na versão da API em uso.

### 5.4 Simulador de taxas ("quanto você recebe")

Ao lado do cadastro, atualiza a cada tecla. Por forma de pagamento: taxa Shopify, taxa do gateway, **valor líquido**, **lucro** e **margem** (se o custo foi informado).

- **Taxa da Shopify por venda com gateway externo** (Shopify Payments não existe no Brasil): Basic **2%**, Grow **1%**, Advanced **0,6%**, Plus **0,2%**.
- **Taxas do gateway** (Mercado Pago etc.) são **individuais de cada lojista** e mudam com o prazo de recebimento → tela **Taxas** onde o lojista copia uma vez: crédito à vista %, crédito 12x sem juros % (pior caso), Pix %, boleto (valor fixo).
- Fórmulas:
  - `taxaShopify = preço × %plano`
  - `taxaGateway = preço × %forma` (boleto: valor fixo)
  - `líquido = preço − taxaShopify − taxaGateway`
  - `lucro = líquido − custo`; `margem = lucro / preço`
  - Cálculo inverso ("quero receber X no cartão"): `preço = X / (1 − %shopify − %gateway)`, arredondado para cima no centavo.
- Exemplo (Basic, cartão 4,98%, Pix 0,99%): produto R$ 79,90, custo R$ 30 → cartão recebe **R$ 74,32** (lucro R$ 44,32); Pix recebe **R$ 77,51**. Para receber R$ 75,00 no cartão → cobrar **R$ 80,63**.
- Aviso exibido: a taxa da Shopify incide sobre o total do pedido (inclui frete), então o valor real pode variar.
- O custo também é enviado à Shopify (`inventoryItem.cost`) para os relatórios de margem dela.

### 5.5 Clientes

- Lista de clientes da loja (nome, e-mail, telefone com link WhatsApp, nº de pedidos, total gasto, data) via Admin API.
- Requer acesso a **dados protegidos de clientes** aprovado no app da Shopify.

### 5.6 Plataforma (só equipe Zum)

- Cadastrar loja: ID, nome, domínio `.myshopify.com`, token Storefront, Client ID/Secret, domínios do site, cor, WhatsApp. Quem cadastra vira dono.
- Trocar credenciais; **testar conexão** (consulta nome/plano da loja).
- Liberar acesso de lojista por e-mail (a pessoa precisa ter entrado uma vez no painel).

---

## 6. Frete

- **Problema relatado:** frete único de R$ 22 para todo o Brasil (cidade vizinha e outro estado pagam igual).
- **Opção 1 — zonas por estado** (sem app, já possível): dividir a zona "Domestic" em zonas (Próximo / Médio / Longe) com preços diferentes; pode ser feito na tela do admin ou via API (`deliveryProfileUpdate` com `zonesToCreate`/`zonesToUpdate`).
- **Opção 2 — frete por CEP e peso (escolhida como alvo):** **CarrierService** — a API do SaaS registra uma URL; no checkout a Shopify envia CEP + itens + pesos; a API responde as opções (tabela própria ou cotação Melhor Envio/Frenet/Correios), preço em **centavos como string**. Produtos já são cadastrados com peso para isso. **[confirmar]** o plano da loja precisa ter **frete calculado por terceiros** liberado (em alguns planos é pago à parte).
- **Entrega local** (cidade e vizinhas, por CEP/raio, preço menor ou motoboy) — configurável no admin da Shopify.
- Endereço de origem atual da Elika: São Sebastião do Paraíso – MG.

---

## 7. Status

### 7.1 Já implementado (API e front compilando, testes passando)

**API (`zum-commerce-api`)** — solução com 6 projetos: `Core.Domain`, `Core.Application`, `Core.Infrastructure`, `Core.Infrastructure.CrossCutting`, `Core.Infrastructure.IoC`, `Core.Web` + `tests/Core.Web.Tests` (xUnit).
- Autenticação Firebase (JWT), regras de acesso por loja, criptografia AES-GCM das credenciais.
- Token Shopify por loja com cache e renovação; cliente GraphQL com retentativa em limite de taxa e renovação de token em 401.
- Endpoints: `GET /api/me`; `POST /api/platform/tenants`; `PUT …/{id}/credentials`; `POST …/{id}/test`; `POST …/{id}/members`; `GET /api/admin/products`; `GET/POST /api/admin/product`; `POST /api/admin/product/status`; `GET /api/admin/customers`; `GET /api/store/customers/me`; `POST /api/store/customers/register`; `GET /health`.
- Erros padronizados `{ "error": "…" }` (400/401/403/404/409/422/502).

**Front (`zum-commerce/frontend`)** — Atomic Design (`atoms`, `molecules`, `organisms`, `templates`, `pages`), `routes`, `contexts`, `data`, `lib`, `types`.
- Vitrine padrão: home, busca, produto com variações, carrinho lateral, login (Google/Facebook/e-mail), cadastro obrigatório, ponte para checkout, minha conta, botão WhatsApp, cor da marca por loja.
- Painel: produtos (lista/cadastro/edição com fotos e grade de variações), simulador de taxas, tela de taxas, clientes, plataforma.
- Regras Firestore/Storage; `firebase.json` com rewrite `/api/**` → Cloud Run.
- 15 testes de lógica (taxas, preço sugerido, variações, formatação) passando.

### 7.2 Pendências — prioridade

**P0 — antes de qualquer loja real**
1. ~~**Publicar produto no canal de vendas**~~ — **feito**: a loja guarda `publicationIds` (escolhidos em Plataforma → Testar conexão); ao salvar, produto ativo vai para `publishablePublish` e rascunho/arquivado para `publishableUnpublish`. App precisa dos escopos `read_publications`/`write_publications`.
2. ~~**Compilar e testar o C#**~~ — **feito** (.NET 10.0.402): build sem erros nem avisos, 38 testes passando. Front: build ok, 15 testes passando.
3. Confirmar que `inventoryQuantities` no `productSet` **atualiza estoque de produto existente**; se não, usar `inventorySetQuantities`.
4. **CORS / proxy para sites sob medida:** liberar o domínio do site em `Cors:Origins` ou fazer o hosting do site redirecionar `/api` para a API.
5. Checklist manual: testar conexão; produto simples e com tamanhos; editar e adicionar foto sem perder as antigas; cadastro de cliente cria Customer (+55); checkout abre com e-mail preenchido.

**P1 — para operar como SaaS**
6. **App público da Zum via OAuth** ("Conectar minha Shopify"), webhook `app/uninstalled`, geração automática do token Storefront.
7. **Integrar o site da Elika** (`elika-tiaras`): trocar mock por Storefront API (coleções na home, catálogo, produto), carrinho real, login/cadastro, checkout real; remover checkout mock. Manter visual e páginas.
8. **Meus pedidos** no site (`GET /api/store/orders/me`).
9. **Destacar na página inicial** pelo painel (gerenciar coleções Destaques/Novidades/Mais vendidos).
10. Extrair o que todo site sob medida usa (Storefront, carrinho, login, cadastro) num pacote **`@zum/commerce-kit`**.

**P2 — crescimento**
11. **Frete por CEP e peso** (CarrierService) — ver 6.
12. **Pedidos no painel** (lista, status, etiqueta).
13. **Worker** para webhooks da Shopify (pedido criado/pago, estoque) e tarefas agendadas → cria `Core.Worker` usando `AddCoreServices()` da IoC.
14. **Integração com sistema próprio/ERP do lojista:** o sistema do cliente vira fonte da verdade de produtos/preço/estoque; API/worker sincroniza com a Shopify; webhook de pedido lança no sistema do cliente. Viabilidade depende de o sistema ter API (ideal), exportar CSV (sincronização periódica) ou nenhum dos dois (acesso a banco, com autorização). Se for ERP conhecido (Bling, Tiny, Omie…), avaliar integração pronta.
15. **Cobrança da mensalidade** dos lojistas (assinatura Mercado Pago/Stripe) e cadastro de loja automatizado.
16. **CI** (GitHub Actions) nos dois repositórios; **testes de integração** da API (`WebApplicationFactory`); tipos do front gerados do **OpenAPI**; **logs/monitoramento por loja** no Cloud Run.
17. Organizar `Core.Application` **por funcionalidade** (`Tenants/`, `Products/`, `Customers/`, depois `Shipping/`, `Orders/`, `Inventory/`, `Billing/`) e **registro automático** dos casos de uso na IoC.

### 7.3 Fora de escopo

- Pagamento próprio (sempre checkout Shopify).
- Login com Instagram.
- **Shopify POS** (venda presencial) — não usado; cliente com caixa próprio integra via item 14.
- Área administrativa dentro dos sites sob medida (lojista usa sempre o painel do SaaS).

---

## 8. Decisões de arquitetura registradas

- **Front e back em repositórios separados** (padrão do Portal Power BI da empresa).
- **Back com 6 projetos** mantido porque estão previstos **worker** (justifica `IoC` compartilhada entre API e worker) e **outros produtos SaaS** (WhatsApp CRM, Hub Financeiro → `CrossCutting` vira pacote `Zum.CrossCutting` compartilhado). Regra: IoC se paga com mais de uma aplicação no mesmo produto; CrossCutting com mais de um produto. Nº de usuários/lojas **não** muda a estrutura de projetos — escala é responsabilidade do Cloud Run/Firestore.
- **Vitrine fala direto com a Storefront API**; API do SaaS só para identidade do cliente e operações com credencial secreta. Não usar MediatR, AutoMapper nem repositório genérico.
- **Firestore** como banco; trocar por relacional mexeria só em `Core.Infrastructure`.

---

## 9. Regras para quem implementar

1. **Nunca** expor Client Secret / token Admin no navegador nem em `tenants/{id}`; segredos só em `tenantSecrets` via `ISecretProtector`.
2. Toda rota de loja valida acesso com `TenantAccessService` (membro ou equipe da plataforma); vitrine valida loja ativa.
3. Dependências entre camadas: `Domain` → nada; `Application` → Domain/CrossCutting (sem Firestore, Shopify, ASP.NET); `Infrastructure` implementa as interfaces; `Web` só orquestra via casos de uso.
4. Mensagens de erro para o usuário **em português**, via `AppException`.
5. Front: componentes no nível certo do Atomic Design; caminhos em `routes/paths.ts`; lógica pura em `lib/` com teste; `localStorage` sempre via `safeStorage`; redirecionamento pós-login só para caminhos internos.
6. Verificar na documentação da Shopify (versão de API usada nas lojas, hoje `2026-10`) antes de usar qualquer mutation/campo novo.
7. Não apagar arquivos do projeto da Elika; alterar com plano aprovado.
