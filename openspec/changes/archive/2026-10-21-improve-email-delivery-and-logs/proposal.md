# Proposal

## Why

O trace dos pedidos mostrou que o e-mail do resultado leva cerca de 3 s por pedido, mesmo com as APIs aquecidas, porque cada e-mail abre a própria conexão com o servidor; numa rodada cheia isso passa de um minuto, e nada desse tempo aparece no trace. Os logs enviados ao painel também repetem o que os spans já mostram e trazem avisos de ambiente que não pedem ação.

## What Changes

- Os e-mails de uma rodada saem pela mesma conexão com o servidor de e-mail, aberta só quando há e-mail a enviar.
- As regras de falha continuam: falha de um pedido não afeta os outros; servidor fora encerra a rodada sem contar tentativa.
- O trace do envio mostra as etapas com o servidor (conexão, autenticação, envio), sem o endereço do destinatário.
- Os logs que só repetem um span (cada chamada HTTP) deixam de ir ao painel; avisos e erros continuam, e o console não muda.
- Saem do painel os avisos de ambiente que se repetem a cada partida: redirecionamento para HTTPS (as APIs só falam HTTP) e licença da biblioteca de mediação.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `order-notification`: os e-mails de uma rodada compartilham uma conexão com o servidor.
- `service-telemetry`: etapas do envio de e-mail no trace e logs sem repetir os spans.

## Impact

- Orders: envio de e-mail (sessão por rodada) e o dispatcher.
- `Frameworker.Observability`: filtros de log para o coletor.
- Program.cs das APIs que redirecionam para HTTPS.
- Testes do envio, do dispatcher e da telemetria; docs (`CLAUDE.md`, README, soluções técnicas).

## Fora do escopo

- Envio paralelo ou em fila externa; mudar o intervalo das rodadas ou o tamanho do lote.
- Aquecimento das APIs na partida (só o primeiro pedido depois de subir é lento).
- Nomes mais descritivos para os spans de HTTP e de banco.
- Mudar a verificação de revogação do certificado do servidor de e-mail.
- Comprar ou configurar licença da biblioteca de mediação.
