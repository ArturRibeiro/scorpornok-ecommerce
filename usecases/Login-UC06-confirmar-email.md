# Login-UC06: Confirmar e-mail e definir senha

**Ator:** Visitante
**Objetivo:** Comprovar que o e-mail é dele e criar a senha de acesso.

## Pré-condição

O visitante recebeu o link por e-mail, depois de se cadastrar (Login-UC05) ou de pedir uma nova senha (Login-UC08).

## Fluxo principal

1. O visitante abre o link recebido.
2. O sistema confirma o e-mail e pede uma senha.
3. O visitante informa a senha e a confirmação.
4. O sistema grava a senha e inicia a sessão.
5. O sistema leva o visitante de volta à loja, na página em que ele estava.

## Fluxos alternativos

- **1a. Link vencido ou já usado:** o sistema informa que o link não vale mais; o visitante pede um novo em "Esqueci minha senha" (Login-UC08).
- **3a. Senha com menos de 8 caracteres:** o sistema recusa e exibe a regra.
- **3b. Confirmação diferente da senha:** o sistema recusa e pede que as duas sejam iguais.

## Regras de negócio

- RN01: A senha tem no mínimo 8 caracteres.
- RN02: Só contas com o e-mail confirmado podem finalizar pedidos (UC03).

## Pós-condição

E-mail confirmado, senha definida e cliente logado.
