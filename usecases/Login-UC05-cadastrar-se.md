# Login-UC05: Cadastrar-se

**Ator:** Visitante
**Objetivo:** Criar uma conta para poder finalizar compras.

## Fluxo principal

1. O visitante aciona "Sign Up" no cabeçalho.
2. O sistema abre a tela de cadastro numa janela sobre a página atual.
3. O visitante informa o e-mail.
4. O sistema cria a conta, ainda sem senha.
5. O sistema envia um link de confirmação para o e-mail informado e pede que o visitante o abra.
6. O visitante segue para confirmar o e-mail (Login-UC06).

## Fluxos alternativos

- **1a. Cadastro a partir do login:** na tela de login (Login-UC07), o visitante escolhe criar uma conta.
- **2a. Visitante fecha a janela:** nenhuma conta é criada e ele continua na mesma página.
- **3a. E-mail em formato inválido:** o sistema indica o erro e não cria a conta.
- **4a. E-mail já cadastrado:** o sistema não cria outra conta e oferece o login (Login-UC07).
- **5a. E-mail não chegou:** o visitante usa "Esqueci minha senha" (Login-UC08) para receber um novo link.

## Regras de negócio

- RN01: O e-mail é o identificador da conta e do login.
- RN02: Cada e-mail tem no máximo uma conta.
- RN03: A senha só é definida depois que o e-mail é confirmado.

## Pós-condição

Conta criada com o e-mail ainda não confirmado; o link de confirmação foi enviado.
