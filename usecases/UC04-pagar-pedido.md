# UC04: Pagar pedido

**Atores:** Cliente, serviço de pagamentos
**Objetivo:** Cobrar o pedido no cartão de crédito e informar o resultado ao cliente.

## Pré-condição

Existe um pedido **Pendente** (UC03).

## Fluxo principal

1. O sistema solicita a cobrança do total do pedido ao serviço de pagamentos.
2. O serviço de pagamentos aprova o pagamento e o registra.
3. O sistema muda o pedido para **Confirmado** e guarda o identificador do pagamento.
4. O sistema informa ao cliente, na tela da finalização, que a compra foi confirmada.
5. O sistema envia ao e-mail do pedido uma mensagem com o número do pedido, o total e a confirmação.

## Fluxos alternativos

- **2a. Pagamento recusado:** o sistema muda o pedido para **Falha no pagamento**, informa o cliente na tela e envia o e-mail com a recusa.
- **2b. Serviço de pagamentos indisponível:** o pedido continua **Pendente**; a cobrança é feita quando o serviço voltar, sem nova ação do cliente.
- **2d. Sem resultado depois do prazo (15 minutos):** o sistema pergunta ao serviço de pagamentos o que aconteceu, a cada minuto, até ter uma resposta:
  - pagamento aprovado ou recusado: o pedido recebe esse resultado, mesmo que ele tenha se perdido no caminho;
  - pagamento ainda em andamento: o pedido continua **Pendente** e é consultado de novo;
  - nenhum pagamento: o pedido fica com **Pagamento não processado**, nada é cobrado e o cliente recebe um e-mail para refazer a compra. Uma cobrança dessa tentativa que chegue depois é recusada sem passar pelo cartão.
- **2e. Serviço de pagamentos indisponível além do prazo:** o pedido continua **Pendente**, e a equipe é avisada pelo monitoramento até ele ter um desfecho.
- **2c. Mesma cobrança solicitada de novo:** o serviço de pagamentos não cobra outra vez e repete o resultado já registrado.
- **4a. Resultado demora mais de 10 segundos:** o sistema informa que o pagamento está pendente e que o resultado será enviado por e-mail.
- **4b. Cliente saiu da loja:** o resultado chega só pelo e-mail.
- **5a. Servidor de e-mail indisponível:** a situação do pedido é atualizada mesmo assim, e o e-mail é enviado quando o servidor voltar.

## Regras de negócio

- RN01: O valor cobrado é o total calculado pelo sistema para o pedido.
- RN02: O pagamento é feito à vista ou parcelado no cartão de crédito.
- RN03: Só pedidos **Pendentes** mudam de situação; um resultado repetido não altera o pedido.
- RN04: O cartão é trocado por um código descartável (token) assim que o cliente confirma a compra; só esse código segue para os pedidos e para a cobrança. O serviço de pagamentos guarda só os 4 últimos dígitos e o titular; o número completo e o CVV não são guardados em lugar nenhum.
- RN09: O token vale 24 horas e uma única cobrança; vencido, desconhecido ou já usado, o pagamento é recusado com o motivo "Token do cartão inválido ou vencido." e o cliente é avisado como numa recusa.
- RN05: Cada resultado gera um único e-mail ao cliente.
- RN06: Só o dono do pedido acompanha o resultado na tela.
- RN07: O sistema de pedidos nunca dá um desfecho ao pagamento por conta própria: depois do prazo, a resposta vem sempre do serviço de pagamentos.
- RN08: **Pagamento não processado** é diferente de **Falha no pagamento**: no primeiro, o problema foi da loja e nada foi cobrado; no segundo, o cartão foi recusado.

## Limitações conhecidas

- O serviço de pagamentos é simulado: recusa cartões terminados em `0000` e aprova os demais.

## Pós-condição

Pedido **Confirmado**, com **Falha no pagamento** ou com **Pagamento não processado**, e o cliente avisado por e-mail.
