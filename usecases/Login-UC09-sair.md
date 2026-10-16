# Login-UC09: Sair

**Ator:** Cliente
**Objetivo:** Encerrar a sessão na loja.

## Pré-condição

O cliente está logado (Login-UC07).

## Fluxo principal

1. O cliente aciona "Sair" no cabeçalho.
2. O sistema encerra a sessão.
3. O sistema volta a exibir "Entrar" e "Cadastrar-se" no cabeçalho.

## Regras de negócio

- RN01: O carrinho é mantido depois de sair.
- RN02: Depois de sair, finalizar um pedido (UC03) exige um novo login.

## Pós-condição

Sessão encerrada; o cliente volta a ser visitante.
