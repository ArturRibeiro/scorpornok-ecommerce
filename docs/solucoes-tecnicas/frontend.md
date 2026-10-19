# Loja (site)

O site onde o cliente navega, monta o carrinho, paga e acompanha o resultado. Cada item indica a sua situação. Visão geral em [README.md](README.md).

## 4. Proteção dos dados do cartão

**Problema:** o site enviava o número completo do cartão e o código de segurança junto com o pedido.

**Solução:** quando o cliente confirma a compra, o site primeiro troca o cartão por um código descartável e só então envia o pedido, apenas com esse código. Para o cliente, nada muda na tela. Se o cartão for inválido, o erro aparece no próprio campo do cartão, e o formulário continua preenchido. Se a parte da loja que valida o cartão estiver fora do ar, o site avisa que não conseguiu validar o cartão, nada é registrado nem cobrado, e o carrinho continua preenchido.

**Situação:** em funcionamento.

Detalhes técnicos: `tokenize-card-data` (arquivada em `openspec/changes/archive/`).

## 5. Carrinho e nova tentativa

**Problema:** o carrinho era esvaziado assim que o pedido era registrado, antes de o pagamento ser aprovado. Se o cartão fosse recusado, o cliente perdia o carrinho e não tinha como tentar outro cartão.

**Solução:**
- O carrinho só é esvaziado quando o pagamento é **aprovado** ou fica **pendente**. No pendente, o pedido já existe e ainda pode ser aprovado, e manter o carrinho levaria o cliente a comprar duas vezes.
- Na **recusa**, o carrinho continua, e a tela oferece "Tentar com outro cartão" enquanto houver tentativas (até três por pedido). O resultado da nova tentativa aparece na mesma tela. Quando as tentativas acabam, a tela orienta a fazer um novo pedido, com o carrinho ainda salvo.

**Situação:** em funcionamento.

Detalhes técnicos: `allow-payment-retry` (arquivada em `openspec/changes/archive/`).

## 2. Pedido não processado

**Problema:** quando o resultado do pagamento não chegava, a tela dizia que o resultado seria enviado por e-mail, mas, se o pedido ficasse parado, esse e-mail nunca chegava.

**Solução:** quando a loja descobre que o pagamento não chegou a ser feito, a tela mostra "Pagamento não processado": nada foi cobrado e o cliente pode refazer a compra. O cliente também recebe essa informação por e-mail.

**Situação:** em funcionamento.

Detalhes técnicos: `handle-stuck-payments` (arquivada em `openspec/changes/archive/`).

## 9. Meu cadastro e checkout preenchido

**Problema:** o cliente digitava o endereço a cada compra, e a loja não tinha onde mostrar ou corrigir os dados dele.

**Solução:**
- O e-mail no topo da loja vira o menu da conta, com **"Meu cadastro"** e "Sair". Na página do cadastro, o cliente vê e altera nome, telefone, CPF e endereço; o e-mail da conta aparece só para leitura. Os erros aparecem no próprio campo, e, se o serviço estiver fora, a loja avisa que não conseguiu salvar e mantém o que foi digitado.
- O checkout pede nome, telefone e CPF e o endereço completo (com número, bairro e o estado numa lista), e abre com esses campos já preenchidos pelo cadastro. O que o cliente muda ali vale só para aquele pedido.
- Sem cadastro, ou com o serviço de cadastro fora do ar, os campos vêm em branco e a compra funciona normalmente.

**Situação:** em funcionamento.

Detalhes técnicos: `add-customer-profile` (arquivada em `openspec/changes/archive/`).

## 10. Meus pedidos

**Problema:** o cliente não tinha onde rever as compras. Se fechasse a tela da confirmação, só sobrava o e-mail, e um pedido recusado não podia mais ser pago com outro cartão.

**Solução:**
- O menu da conta ganha **"Meus pedidos"**, e a confirmação da compra ganha o link "Ver meus pedidos".
- A lista mostra número, data, situação, total, quantidade de itens e a foto de um produto, do mais recente para o mais antigo, com "Carregar mais" para os pedidos mais antigos. Quem ainda não comprou vê "Você ainda não fez nenhuma compra", com um caminho de volta para a loja.
- Ao abrir um pedido, o cliente vê os itens (com preço unitário, quantidade, subtotal e o desconto, quando houver), o total, o endereço de entrega e a situação: "Aguardando pagamento", "Pago", "Pagamento recusado" ou "Pagamento não processado".
- Um pedido aguardando pagamento se atualiza sozinho quando o resultado chega. Um pedido recusado mostra quantas tentativas restam e permite pagar com outro cartão ali mesmo; sem tentativas, a tela orienta a refazer a compra.
- Um número de pedido que não é do cliente aparece como "Pedido não encontrado".

**Situação:** em funcionamento.

Detalhes técnicos: `add-order-history` (arquivada em `openspec/changes/archive/`).
