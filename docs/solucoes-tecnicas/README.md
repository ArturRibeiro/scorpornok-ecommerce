# Problemas e soluções da loja

Esta pasta explica, em linguagem de negócio, o que podia dar errado quando o cliente pagava um pedido, o que faltava para conhecer o cliente e mostrar a ele as próprias compras, e o que fizemos para cada caso. Os documentos estão divididos pela parte da loja que cada solução afeta.

**Situação:** todas as soluções estão **em funcionamento**. Os detalhes técnicos de cada uma ficam no histórico do OpenSpec (`openspec/changes/archive/`), indicados ao fim de cada item.

## Resumo

| # | Problema | Solução | Situação |
|---|---|---|---|
| 1 | Uma falha interna no meio do pagamento pode cobrar o cliente duas vezes | O sistema anota a cobrança antes de fazê-la e o meio de pagamento reconhece um pedido repetido, então cada tentativa é cobrada no máximo uma vez | Em funcionamento |
| 2 | Um pedido pode ficar "em processamento" para sempre, às vezes com o dinheiro já cobrado | Depois de um prazo, a loja verifica o que aconteceu com o pagamento e dá uma resposta definitiva ao cliente; a equipe é avisada quando algo trava | Em funcionamento |
| 3 | Uma instabilidade de poucos segundos já faz a loja desistir de processar o pagamento | A loja insiste por alguns minutos, com pausas cada vez maiores, antes de desistir | Em funcionamento |
| 4 | Os dados completos do cartão, inclusive o código de segurança, ficam guardados em sistemas internos | O cartão vira um código descartável assim que o cliente o digita; os dados reais não circulam nem ficam guardados | Em funcionamento |
| 5 | Quando o cartão é recusado, o cliente perde o carrinho e não pode tentar outro cartão | O carrinho é mantido e o cliente pode tentar pagar o mesmo pedido com outro cartão | Em funcionamento |
| 6 | Se o e-mail com o resultado não pode ser entregue, a loja tenta sem parar e o cliente nunca é avisado | A loja tenta algumas vezes, com pausas, e avisa a equipe quando não consegue entregar | Em funcionamento |
| 7 | A loja aceita o preço que chega do navegador, que pode ser alterado | O preço de cada produto é conferido com o catálogo no momento da compra | Em funcionamento |
| 8 | Dois pedidos podiam receber o mesmo número, e um pedido novo podia ser confundido com um antigo e confirmado sem pagamento | Cada pedido recebe um número sorteado que não se repete, e a loja recusa registrar um número já usado | Em funcionamento |
| 9 | A loja não guardava os dados do cliente, que digitava o endereço de novo a cada compra | O cliente tem um cadastro (criado na primeira compra), o checkout já vem preenchido e cada pedido guarda o seu endereço de entrega no histórico | Em funcionamento |
| 10 | O cliente não tinha onde rever os pedidos já feitos, e quem fechava a tela da confirmação perdia o caminho para pagar de novo um pedido recusado | "Meus pedidos" lista as compras do cliente, mostra cada pedido como foi comprado, acompanha o pagamento e permite pagar de novo um pedido recusado | Em funcionamento |
| 11 | A loja fala com cada API por um endereço e um formato próprios, e cada nova tela multiplica as chamadas | Um ponto de entrada único (GraphQL) junta as APIs, identifica o cliente e barra consultas abusivas; catálogo, cadastro, pedidos e a validação do cartão passam por ele | Em funcionamento (4ª etapa) |

## Por parte da loja

- [Pagamentos](payments.md): itens 1, 2, 3, 4, 5 e 11.
- [Pedidos](orders.md): itens 2, 3, 5, 6, 7, 8, 9, 10 e 11.
- [Loja (site)](frontend.md): itens 2, 4, 5, 9, 10 e 11.
- [Cadastro do cliente](customers.md): itens 9 e 11.
- [Catálogo](catalog.md): itens 7 e 11.

## Ordem de implementação

| Ordem | Solução | Situação |
|---|---|---|
| 1 | Preço conferido com o catálogo (7) | Em funcionamento |
| 2 | Número do pedido que não se repete (8) | Em funcionamento |
| 3 | Cobrança única (1) | Em funcionamento |
| 4 | Insistência por alguns minutos (3) | Em funcionamento |
| 5 | Pedidos presos (2) | Em funcionamento |
| 6 | Proteção do cartão (4) | Em funcionamento |
| 7 | Limite no envio de e-mail (6) | Em funcionamento |
| 8 | Nova tentativa com outro cartão (5) | Em funcionamento |
| 9 | Cadastro do cliente (9) | Em funcionamento |
| 10 | Meus pedidos (10) | Em funcionamento |
| 11 | Ponto de entrada único, 1ª etapa: catálogo (11) | Em funcionamento |
| 12 | Ponto de entrada único, 2ª etapa: cadastro e identificação do cliente (11) | Em funcionamento |
| 13 | Ponto de entrada único, 3ª etapa: pedidos (11) | Em funcionamento |
| 14 | Ponto de entrada único, 4ª etapa: validação do cartão (11) | Em funcionamento |

As soluções foram implementadas nesta ordem. O número que não se repete veio antes da cobrança única porque a cobrança única identifica cada cobrança pelo número do pedido: com números repetidos, ela confundiria dois pedidos. A cobrança única vem cedo porque várias outras soluções dependem dela, e a nova tentativa com outro cartão vem depois porque depende da cobrança única e da proteção do cartão. O cadastro do cliente veio depois: ele não muda o pagamento, só acrescenta os dados de quem compra. "Meus pedidos" veio depois porque reaproveita a nova tentativa com outro cartão e só lê os pedidos, sem mudar o registro nem a cobrança. O ponto de entrada único começou pelo catálogo, que só é lido e não exige login; depois veio o cadastro, que trouxe a identificação do cliente no ponto de entrada, depois os pedidos, que reaproveitam essa identificação, e por último a validação do cartão, que exigiu cuidado extra porque os dados do cartão passam a atravessar o ponto de entrada. O acompanhamento do pagamento na tela ainda usa o canal de antes.
