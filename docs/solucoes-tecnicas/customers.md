# Cadastro do cliente

A parte da loja que guarda os dados de quem compra: nome, telefone, CPF, o endereço de entrega e o histórico dos endereços usados nos pedidos. Cada item indica a sua situação. Visão geral em [README.md](README.md).

## 9. Cliente sem cadastro

**Problema:** a conta do cliente tinha só o e-mail. A loja não guardava nome, telefone, CPF nem endereço, e o cliente digitava o endereço de novo a cada compra.

**Solução:**
- O cliente logado tem a página **"Meu cadastro"**, no menu da conta, onde vê e altera nome, telefone, CPF e endereço de entrega. Só ele vê e altera o próprio cadastro.
- Todos os campos são obrigatórios, menos o complemento do endereço, e são conferidos antes de salvar (CPF válido, telefone com DDD, CEP com 8 dígitos e um estado brasileiro). Se algo estiver errado, nada é salvo e o campo com problema é indicado.
- Na **primeira compra**, o cadastro é criado com os dados informados no checkout. Um cadastro que já existe não muda pela compra.
- Cada pedido registra o seu endereço de entrega num **histórico** do cliente, mesmo quando ele é diferente do endereço do cadastro.
- O cadastro não atrapalha a compra: se esse serviço estiver fora do ar, o pedido é registrado normalmente, e o histórico (ou o cadastro da primeira compra) é atualizado quando ele volta.

**Situação:** em funcionamento.

Detalhes técnicos: `add-customer-profile` (arquivada em `openspec/changes/archive/`).
