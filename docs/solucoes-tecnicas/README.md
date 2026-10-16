# Falhas no pagamento: problemas e soluções

Esta pasta explica, em linguagem de negócio, o que pode dar errado quando o cliente paga um pedido e o que propomos para cada caso. Os documentos estão divididos pela parte da loja que cada solução afeta.

**Situação:** todas as soluções são **propostas** e ainda não estão em funcionamento. Os detalhes técnicos de cada uma ficam nas propostas do OpenSpec (`openspec/changes/`), indicadas ao fim de cada item.

## Resumo

| # | Problema | Solução |
|---|---|---|
| 1 | Uma falha interna no meio do pagamento pode cobrar o cliente duas vezes | O sistema anota a cobrança antes de fazê-la e o meio de pagamento reconhece um pedido repetido, então cada tentativa é cobrada no máximo uma vez |
| 2 | Um pedido pode ficar "em processamento" para sempre, às vezes com o dinheiro já cobrado | Depois de um prazo, a loja verifica o que aconteceu com o pagamento e dá uma resposta definitiva ao cliente; a equipe é avisada quando algo trava |
| 3 | Uma instabilidade de poucos segundos já faz a loja desistir de processar o pagamento | A loja insiste por alguns minutos, com pausas cada vez maiores, antes de desistir |
| 4 | Os dados completos do cartão, inclusive o código de segurança, ficam guardados em sistemas internos | O cartão vira um código descartável assim que o cliente o digita; os dados reais não circulam nem ficam guardados |
| 5 | Quando o cartão é recusado, o cliente perde o carrinho e não pode tentar outro cartão | O carrinho é mantido e o cliente pode tentar pagar o mesmo pedido com outro cartão |
| 6 | Se o e-mail com o resultado não pode ser entregue, a loja tenta sem parar e o cliente nunca é avisado | A loja tenta algumas vezes, com pausas, e avisa a equipe quando não consegue entregar |
| 7 | A loja aceita o preço que chega do navegador, que pode ser alterado | O preço de cada produto é conferido com o catálogo no momento da compra |

## Por parte da loja

- [Pagamentos](payments.md): itens 1, 2, 3, 4 e 5.
- [Pedidos](orders.md): itens 2, 3, 5, 6 e 7.
- [Loja (site)](frontend.md): itens 2, 4 e 5.
- [Catálogo](catalog.md): item 7.

## Ordem sugerida

1. Preço conferido com o catálogo (7)
2. Cobrança única (1)
3. Insistência por alguns minutos (3)
4. Pedidos presos (2)
5. Proteção do cartão (4)
6. Limite no envio de e-mail (6)
7. Nova tentativa com outro cartão (5)

A cobrança única vem cedo porque várias outras soluções dependem dela, e a nova tentativa com outro cartão vem por último porque depende da cobrança única e da proteção do cartão.
