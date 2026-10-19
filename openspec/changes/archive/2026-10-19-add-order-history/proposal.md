# Proposal

## Why

Hoje o cliente compra, recebe por e-mail o resultado do pagamento e não tem onde rever os pedidos que já fez. Quem fecha a tela da confirmação também perde o caminho para pagar de novo um pedido recusado.

## What Changes

- **Meus pedidos**: lista dos pedidos do cliente logado, do mais recente para o mais antigo, com número, data, situação, total, quantidade de itens e a foto de um produto; carrega aos poucos ("Carregar mais") e tem estado vazio com caminho de volta à loja.
- **Detalhe do pedido**: itens (nome, foto, preço unitário, desconto, quantidade e subtotal), total, endereço de entrega e situação do pagamento, como registrados na compra, mesmo que o produto tenha mudado de preço ou saído da loja.
- **Situação**: "Aguardando pagamento" (o detalhe se atualiza sozinho quando o resultado chega), "Pago", "Pagamento recusado" (com as tentativas restantes) e "Pagamento não processado" (nada cobrado; refazer a compra).
- **Nova tentativa pelo pedido**: com o pagamento recusado e tentativas restantes, o cliente paga de novo com outro cartão direto do detalhe; esgotadas, a tela orienta a refazer a compra.
- **Privacidade**: só o dono vê o pedido, com o e-mail confirmado; pedido de outro cliente ou inexistente aparece como "não encontrado", e nenhum dado sensível (CPF, telefone, cartão, detalhes internos do pagamento e do e-mail) é exibido.
- **Acesso**: "Meus pedidos" no menu da conta e link "Ver meus pedidos" na confirmação da compra.

## Capabilities

### New Capabilities
- `order-history`: consulta dos pedidos do cliente logado (lista, detalhe, situação, nova tentativa a partir do pedido, privacidade e acesso).

### Modified Capabilities
- Nenhuma. A nova tentativa reaproveita a regra já definida em `order-placement`; o link na confirmação fica descrito em `order-history`.

## Impact

- **Orders**: duas consultas novas, só leitura, para o dono do pedido; nenhuma mudança no registro, no pagamento ou nas mensagens.
- **Front-end**: duas páginas novas, item no menu da conta (desktop e móvel), link na confirmação e o formulário de nova tentativa compartilhado entre a confirmação e o detalhe.
- **Docs**: novo caso de uso de consulta de pedidos e o `CLAUDE.md` (raiz e front-end).

## Fora do escopo

- Cancelamento, devolução, rastreio da entrega e nota fiscal.
- "Comprar de novo" a partir de um pedido.
- Filtros por situação ou data e busca pelo número.
- Atualização ao vivo da lista (só o detalhe acompanha o pagamento).
