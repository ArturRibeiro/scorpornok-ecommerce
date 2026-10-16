# Proposal

## Why

O e-mail do resultado do pagamento é tentado a cada poucos segundos, sem limite nem intervalo crescente. Um erro permanente (destinatário recusado, endereço inválido) gera tentativas infinitas, enche o log e o cliente nunca é avisado; ninguém fica sabendo. Além disso, um pedido com erro trava a rodada inteira, atrasando o e-mail dos pedidos seguintes.

## What Changes

- **Orders**:
  - Cada pedido passa a ter um número de tentativas de envio e a data da próxima tentativa, com intervalo crescente entre elas.
  - Erro do destinatário (endereço inválido ou recusado pelo servidor): o envio daquele pedido é encerrado como falha definitiva, sem novas tentativas.
  - Erro temporário daquele e-mail: nova tentativa com intervalo crescente; esgotado o limite de tentativas, falha definitiva.
  - Erro do servidor de e-mail (fora do ar, autenticação, TLS): não conta tentativa para os pedidos; o envio espera com intervalo crescente e é retomado quando o servidor voltar.
  - Um pedido com erro não impede o envio dos demais na mesma rodada.
  - Falha definitiva é registrada como erro no log, com o número do pedido.
  - Nova verificação de saúde do envio de e-mail, separada da prontidão da API.
- **Banco do Orders**: novos campos no pagamento do pedido; como não há migrations, o volume do `orders-db` precisa ser recriado.
- **Docs**: o `CLAUDE.md` passa a descrever o limite, o intervalo crescente e a nova verificação.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `order-notification`: o envio do e-mail passa a ter limite de tentativas, intervalo crescente e falha definitiva observável.

## Impact

- **Orders**: `OrderPayment`, `Order`, `OrderRepository`, `OrderEmailDispatcher`, `SmtpOrderEmailSender`, `SmtpOptions`, health checks do `Orders.Web.Api` e testes.
- **Dados**: recriar o volume do `orders-db` apaga os pedidos de teste.
- **Payments e front-end**: sem mudanças.

## Fora do escopo

- Reenvio manual ou automático de e-mails em falha definitiva (fica no banco para consulta).
- Eliminar o e-mail duplicado quando o processo cai entre enviar e marcar.
- Alertas externos (e-mail para a equipe, Slack); a falha fica no log e na verificação de saúde.
- Avisar o cliente por outro canal quando o e-mail falha.
