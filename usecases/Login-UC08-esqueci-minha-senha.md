# Login-UC08: Esqueci minha senha

**Ator:** Visitante
**Objetivo:** Recuperar o acesso à conta quando não lembra a senha.

## Fluxo principal

1. Na tela de login (Login-UC07), o visitante aciona "Esqueceu sua senha?".
2. O visitante informa o e-mail da conta.
3. O sistema envia um link para definir uma nova senha e informa que o e-mail foi enviado.
4. O visitante abre o link recebido.
5. O visitante informa a nova senha e a confirmação.
6. O sistema grava a nova senha e inicia a sessão.

## Fluxos alternativos

- **3a. E-mail sem conta:** o sistema exibe a mesma mensagem de envio, mas nenhum e-mail é enviado.
- **3b. Conta ainda não confirmada:** o link também confirma o e-mail (como no Login-UC06).
- **4a. Link vencido ou já usado:** o sistema informa que o link não vale mais e o visitante pede um novo.
- **5a. Senha com menos de 8 caracteres ou confirmação diferente:** o sistema recusa e exibe a regra.

## Regras de negócio

- RN01: O sistema não revela se um e-mail tem conta.
- RN02: A nova senha segue as mesmas regras do Login-UC06 (mínimo de 8 caracteres).
- RN03: O link serve uma única vez e vence depois de um tempo.

## Pós-condição

Nova senha definida e cliente logado.
