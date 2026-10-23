# Configuração e deploy

## 1. Firebase

1. Crie um projeto no plano **Blaze** (o Cloud Run exige faturamento).
2. **Authentication**: ative Google, E-mail/senha e Facebook. O Facebook pede App ID e App Secret de um app em developers.facebook.com. Adicione os domínios das lojas em *Domínios autorizados*.
3. Ative **Firestore** e **Storage** na região `southamerica-east1`.
4. As regras (`firestore.rules`, `storage.rules`) estão no repositório do front e são publicadas com `firebase deploy`.

## 2. Shopify (para cada loja)

1. **Admin API**: no admin da loja, vá em Configurações → Apps → Desenvolver apps → **Dev Dashboard**. Crie o app dentro da organização da loja, defina os escopos, faça o Release, instale na loja e copie o **Client ID** e o **Client Secret**.
   - Escopos: `read_products`, `write_products`, `read_inventory`, `write_inventory`, `read_locations`, `read_customers`, `write_customers`, `write_files`.
   - Peça acesso a **dados protegidos de clientes** (nome, e-mail, telefone).
   - O *client credentials* só funciona quando o app e a loja estão na mesma organização. Para lojas de outras organizações, use o modo `access_token` (app OAuth, fase 2).
2. **Storefront API**: instale o canal **Headless** e copie o token público.
3. **Pagamentos**: configure o gateway (ex.: Mercado Pago).
4. **Tema da Shopify**: proteja com senha ou redirecione para o site, para que a venda aconteça pela vitrine da plataforma.

## 3. Chave de criptografia

```bash
openssl rand -base64 32 | gcloud secrets create zum-secrets-key --data-file=-
```

Não troque essa chave depois de cadastrar lojas: os segredos já gravados deixariam de abrir. Se precisar trocar, recadastre as credenciais de cada loja.

## 4. Deploy no Cloud Run

```bash
cd backend
gcloud run deploy zum-commerce-api \
  --source . \
  --region southamerica-east1 \
  --allow-unauthenticated \
  --set-env-vars "Firebase__ProjectId=SEU_PROJETO,Platform__AdminEmails=seu@email.com,Cors__Origins=https://seu-projeto.web.app" \
  --set-secrets "Secrets__Key=zum-secrets-key:latest"
```

Papéis da service account do Cloud Run:
- **Cloud Datastore User**: Firestore
- **Firebase Authentication Admin**: liberar acesso por e-mail
- **Secret Manager Secret Accessor**: chave de criptografia

O `--allow-unauthenticated` é necessário porque quem autentica é a própria API, com o token do Firebase. O Firebase Hosting do front redireciona `/api/**` para esse serviço.

## 5. Primeiro uso

1. Abra `/admin` com o e-mail de `Platform__AdminEmails`. Pelo Google o e-mail já vem verificado.
2. Na aba **Plataforma**, cadastre a loja e clique em **Testar conexão**.
3. Em **Taxas**, configure o plano da Shopify e o gateway.
4. Para liberar o lojista: ele entra uma vez em `/admin`, e depois você libera o e-mail dele na aba Plataforma.

## Checklist do primeiro teste

- [ ] `dotnet build` e `dotnet test` sem erros
- [ ] Testar conexão retorna o nome da loja
- [ ] Criar produto simples e com tamanhos; conferir preço, custo, estoque e peso no admin da Shopify
- [ ] Editar produto e adicionar foto sem perder as antigas
- [ ] Editar estoque de produto existente
- [ ] Cadastro de cliente na vitrine cria o Customer na Shopify (telefone +55…)
