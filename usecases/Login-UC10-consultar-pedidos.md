# Login-UC10: Consultar meus pedidos

**Ator:** Cliente
**Objetivo:** Rever os pedidos já feitos, com a situação do pagamento de cada um, e pagar de novo um pedido recusado.

## Pré-condição

O cliente está logado (Login-UC07), com o e-mail confirmado.

## Fluxo principal

1. O cliente aciona "Meus pedidos" no menu da conta, ou "Ver meus pedidos" na confirmação da compra (UC03).
2. O sistema lista os pedidos do cliente, do mais recente para o mais antigo, com número, data, situação, total, quantidade de itens e a foto de um dos produtos.
3. O cliente abre um pedido.
4. O sistema mostra os itens (nome, foto, preço unitário, desconto, quantidade e subtotal), o total, o endereço de entrega e a situação do pagamento.

## Fluxos alternativos

- **1a. Sem sessão:** o sistema pede o login (Login-UC07) e, depois dele, mostra a lista.
- **2a. Nenhuma compra:** o sistema informa "Você ainda não fez nenhuma compra" e oferece voltar à loja.
- **2b. Mais de 10 pedidos:** o sistema mostra os 10 mais recentes e "Carregar mais", que acrescenta os seguintes até o fim, sem repetir nenhum.
- **2c. Serviço de pedidos indisponível:** o sistema informa que não foi possível carregar os pedidos e oferece tentar de novo.
- **3a. Pedido de outro cliente ou inexistente** (endereço digitado ou compartilhado): o sistema informa "Pedido não encontrado", sem dizer se o número existe.
- **4a. Pagamento em andamento:** a situação é "Aguardando pagamento", e a página muda sozinha para o resultado quando ele chega (UC04). Se não for possível acompanhar, o sistema avisa que o resultado vai por e-mail.
- **4b. Pagamento recusado com tentativas restantes:** o sistema mostra quantas restam e permite pagar com outro cartão (UC04, RN10); a situação volta a "Aguardando pagamento".
- **4c. Pagamento recusado sem tentativas:** o sistema informa que é preciso refazer a compra.
- **4d. Pagamento não processado:** o sistema informa que nada foi cobrado e orienta refazer a compra.

## Regras de negócio

- RN01: O cliente vê só os próprios pedidos.
- RN02: O pedido aparece como foi comprado: um produto que mudou de preço ou saiu da loja, ou um endereço alterado no cadastro, não muda o pedido.
- RN03: A tela não mostra CPF, telefone, dados do cartão nem detalhes internos do pagamento ou do envio do e-mail.
- RN04: Situações exibidas: "Aguardando pagamento" (Pendente), "Pago" (Confirmado), "Pagamento recusado" (Falha no pagamento) e "Pagamento não processado".

## Pós-condição

Nenhuma mudança no pedido, exceto quando o cliente paga de novo um pedido recusado (4b).
