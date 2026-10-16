# Falhas no pagamento: problemas e soluções

Esta pasta explica, em linguagem de negócio, o que pode dar errado quando o cliente paga um pedido e o que fizemos ou propomos para cada caso. Os documentos estão divididos pela parte da loja que cada solução afeta.

Cada solução indica a sua **situação**:
- **Em funcionamento:** já está na loja. Os detalhes técnicos ficam no histórico do OpenSpec (`openspec/changes/archive/`).
- **Proposta:** planejada, mas ainda não implementada. Os detalhes técnicos ficam em `openspec/changes/`.

## Resumo

| # | Problema | Solução | Situação |
|---|---|---|---|
| 1 | Uma falha interna no meio do pagamento pode cobrar o cliente duas vezes | O sistema anota a cobrança antes de fazê-la e o meio de pagamento reconhece um pedido repetido, então cada tentativa é cobrada no máximo uma vez | Em funcionamento |
| 2 | Um pedido pode ficar "em processamento" para sempre, às vezes com o dinheiro já cobrado | Depois de um prazo, a loja verifica o que aconteceu com o pagamento e dá uma resposta definitiva ao cliente; a equipe é avisada quando algo trava | Em funcionamento |
| 3 | Uma instabilidade de poucos segundos já faz a loja desistir de processar o pagamento | A loja insiste por alguns minutos, com pausas cada vez maiores, antes de desistir | Em funcionamento |
| 4 | Os dados completos do cartão, inclusive o código de segurança, ficam guardados em sistemas internos | O cartão vira um código descartável assim que o cliente o digita; os dados reais não circulam nem ficam guardados | Em funcionamento |
| 5 | Quando o cartão é recusado, o cliente perde o carrinho e não pode tentar outro cartão | O carrinho é mantido e o cliente pode tentar pagar o mesmo pedido com outro cartão | Proposta |
| 6 | Se o e-mail com o resultado não pode ser entregue, a loja tenta sem parar e o cliente nunca é avisado | A loja tenta algumas vezes, com pausas, e avisa a equipe quando não consegue entregar | Proposta |
| 7 | A loja aceita o preço que chega do navegador, que pode ser alterado | O preço de cada produto é conferido com o catálogo no momento da compra | Em funcionamento |
| 8 | Dois pedidos podiam receber o mesmo número, e um pedido novo podia ser confundido com um antigo e confirmado sem pagamento | Cada pedido recebe um número sorteado que não se repete, e a loja recusa registrar um número já usado | Em funcionamento |

## Por parte da loja

- [Pagamentos](payments.md): itens 1, 2, 3, 4 e 5.
- [Pedidos](orders.md): itens 2, 3, 5, 6, 7 e 8.
- [Loja (site)](frontend.md): itens 2, 4 e 5.
- [Catálogo](catalog.md): item 7.

## Ordem de implementação

| Ordem | Solução | Situação |
|---|---|---|
| 1 | Preço conferido com o catálogo (7) | Em funcionamento |
| 2 | Número do pedido que não se repete (8) | Em funcionamento |
| 3 | Cobrança única (1) | Em funcionamento |
| 4 | Insistência por alguns minutos (3) | Em funcionamento |
| 5 | Pedidos presos (2) | Em funcionamento |
| 6 | Proteção do cartão (4) | Em funcionamento |
| 7 | Limite no envio de e-mail (6) | Próxima |
| 8 | Nova tentativa com outro cartão (5) | Proposta |

O número que não se repete veio antes da cobrança única porque a cobrança única identifica cada cobrança pelo número do pedido: com números repetidos, ela confundiria dois pedidos. A cobrança única vem cedo porque várias outras soluções dependem dela, e a nova tentativa com outro cartão vem por último porque depende da cobrança única e da proteção do cartão.
