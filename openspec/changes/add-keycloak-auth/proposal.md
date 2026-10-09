# Proposal

## Why

Qualquer pessoa finaliza uma compra: o cliente é um GUID anônimo e o e-mail é digitado sem verificação. O resultado pode ir para o e-mail errado, e quem souber um número de pedido acompanha o pagamento dele.

## What Changes

- Keycloak como provedor de identidade, com o formulário padrão de login e cadastro.
- O checkout exige login e usa o e-mail da conta.
- O Orders aceita pedidos e acompanhamento só de cliente autenticado com e-mail confirmado. **BREAKING** para quem chama a API sem token.

## Capabilities

### New Capabilities
- `customer-auth`: cadastro, confirmação do e-mail, login e logout.

### Modified Capabilities
- `checkout`: login exigido; sai a identificação anônima e o campo de e-mail.
- `order-placement`: cliente autenticado exigido; cliente e e-mail vêm do token.

## Impact

- **Infra**: Keycloak e `keycloak-db` no compose.
- **Dependências**: JwtBearer no Orders, cliente OIDC no front-end.
- **Dados**: sem mudança; pedidos anônimos antigos ficam como estão.

## Fora do escopo

- Formulário de login próprio, login social e papéis.
- Recuperação de senha e tema customizados.
- "Meus pedidos" e perfil do cliente.
- Autenticação no Catalog e entre serviços.
- Keycloak em produção.
