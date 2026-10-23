# Endpoints

Todas as rotas exigem `Authorization: Bearer <ID token do Firebase>`, exceto `/health`.
As rotas de loja exigem também o cabeçalho `X-Tenant-Id: <id da loja>`.

| Método e rota | Quem pode | Função |
|---|---|---|
| `GET /api/me` | qualquer usuário logado | Se é da equipe da plataforma e quais lojas administra |
| `POST /api/platform/tenants` | equipe da plataforma | Cadastra loja (quem cadastra vira dono) |
| `PUT /api/platform/tenants/{id}/credentials` | equipe da plataforma | Troca Client ID/Secret ou define Access Token |
| `POST /api/platform/tenants/{id}/test` | equipe da plataforma | Testa a conexão com a Shopify e lista os canais de vendas |
| `PUT /api/platform/tenants/{id}/publications` | equipe da plataforma | Define os canais onde os produtos são publicados (`{ "publicationIds": [...] }`) |
| `POST /api/platform/tenants/{id}/members` | equipe da plataforma | Libera acesso ao painel por e-mail |
| `GET /api/admin/products?q=&after=` | membro da loja | Lista produtos (paginação por cursor) |
| `GET /api/admin/product?id=` | membro da loja | Detalhe do produto (`id` = gid da Shopify) |
| `POST /api/admin/product` | membro da loja | Cria (sem `id`) ou atualiza produto |
| `POST /api/admin/product/status?id=` | membro da loja | Ativa, deixa em rascunho ou arquiva |
| `GET /api/admin/customers?q=&after=` | membro da loja | Lista clientes da Shopify |
| `GET /api/store/customers/me` | cliente logado | Cadastro do cliente nesta loja (404 = ainda não cadastrado) |
| `POST /api/store/customers/register` | cliente logado | Completa o cadastro e cria o Customer na Shopify |
| `GET /health` | público | Processo no ar |

## Exemplo: produto com tamanhos

```json
POST /api/admin/product
X-Tenant-Id: elika

{
  "id": null,
  "title": "Tiara de pérolas",
  "descriptionHtml": "<p>Tiara forrada com pérolas.</p>",
  "vendor": "Elika",
  "productType": "Tiaras",
  "tags": ["novidade"],
  "status": "ACTIVE",
  "options": [{ "name": "Tamanho", "values": ["P", "M"] }],
  "variants": [
    { "id": null, "options": { "Tamanho": "P" }, "price": 79.9, "compareAtPrice": 99.9, "cost": 30, "sku": "TIA-P", "quantity": 10, "weightGrams": 40 },
    { "id": null, "options": { "Tamanho": "M" }, "price": 79.9, "compareAtPrice": 99.9, "cost": 30, "sku": "TIA-M", "quantity": 8, "weightGrams": 45 }
  ],
  "images": [{ "id": null, "url": "https://firebasestorage.googleapis.com/...", "alt": "Tiara de pérolas" }]
}
```

Produto sem variação: `"options": []` e uma única variação com `"options": {}`.
