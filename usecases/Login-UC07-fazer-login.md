# Login-UC07: Fazer login

**Ator:** Visitante
**Objetivo:** Entrar na loja com a sua conta para poder finalizar compras.

## Pré-condição

O visitante tem uma conta com o e-mail confirmado (Login-UC05 e Login-UC06).

## Fluxo principal

1. O visitante aciona "Entrar" no cabeçalho.
2. O sistema abre a tela de login numa janela sobre a página atual.
3. O visitante informa o e-mail e a senha.
4. O sistema confere as credenciais e inicia a sessão.
5. O sistema fecha a janela e exibe o e-mail do cliente no cabeçalho, na mesma página.

## Fluxos alternativos

- **1a. Login pedido pelo sistema:** ao finalizar o pedido sem sessão (UC03) ou com a sessão expirada, o sistema abre o login sozinho e, depois dele, volta à finalização.
- **3a. Não tem conta:** o visitante escolhe criar uma conta (Login-UC05).
- **3b. Esqueceu a senha:** o visitante aciona "Esqueceu sua senha?" (Login-UC08).
- **4a. E-mail ou senha incorretos:** o sistema não inicia a sessão e exibe uma mensagem de erro.
- **5a. Visitante fecha a janela numa página que exige login:** o sistema exibe "Entre para continuar", com as opções de entrar e de se cadastrar.

## Regras de negócio

- RN01: A mensagem de erro é a mesma para e-mail inexistente e para senha errada, para não revelar quais e-mails têm conta.
- RN02: A sessão é renovada automaticamente enquanto for válida; o cliente não precisa entrar de novo a cada acesso.
- RN03: O carrinho é mantido ao entrar.

## Pós-condição

Cliente logado.
