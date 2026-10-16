# UC03: Finalizar pedido

**Ator:** Cliente
**Objetivo:** Transformar o carrinho em um pedido de compra.

## Pré-condições

- O carrinho tem pelo menos um item (UC02).
- O cliente está logado (Login-UC07) com o e-mail confirmado (Login-UC06).

## Fluxo principal

1. O cliente solicita a finalização da compra.
2. O sistema exibe os itens, o total e o e-mail da conta, para onde vai o resultado do pagamento.
3. O cliente informa o endereço de entrega.
4. O cliente informa os dados do cartão de crédito e o número de parcelas.
5. O cliente confirma o pedido.
6. O sistema registra o pedido com a situação **Pendente**, na conta do cliente, e gera o número do pedido.
7. O sistema exibe o número do pedido, informa que o pagamento está em processamento e esvazia o carrinho.
8. O sistema segue para o pagamento (UC04).

## Fluxos alternativos

- **1a. Cliente sem sessão:** o sistema pede o login (Login-UC07) e, depois dele, volta à finalização com o carrinho como estava.
- **1b. Carrinho vazio:** o sistema leva o cliente ao carrinho.
- **2a. Editar o carrinho:** o cliente volta ao carrinho (UC02) com os mesmos itens.
- **3a. Endereço incompleto:** o sistema indica os campos obrigatórios que faltam e não envia o pedido.
- **4a. Cartão inválido:** número, CVV ou validade fora das regras; o sistema indica o campo e não envia o pedido.
- **6a. Pedido recusado pelo sistema:** o sistema exibe os motivos e mantém o carrinho e os dados preenchidos.
- **6b. Sessão expirada:** o pedido não é registrado; o sistema pede o login de novo e mantém o carrinho e os dados preenchidos.
- **6c. E-mail da conta não confirmado:** o pedido não é registrado.
- **6d. Serviço de pedidos indisponível:** o sistema informa que não conseguiu enviar o pedido e mantém o carrinho e os dados preenchidos.
- **6e. Produto que não existe mais no catálogo:** o pedido não é registrado; o sistema informa qual produto não foi encontrado e mantém o carrinho e os dados preenchidos.
- **6f. Catálogo indisponível:** o pedido não é registrado nem cobrado; o sistema informa que não conseguiu enviar o pedido e mantém o carrinho e os dados preenchidos.

## Regras de negócio

- RN01: O endereço de entrega exige rua, cidade, estado, país e CEP.
- RN02: O cartão exige titular, número de 13 a 19 dígitos, mês e ano de validade (não vencida), CVV de 3 ou 4 dígitos e parcelas (padrão: 1).
- RN03: O pedido pertence ao cliente logado e usa o e-mail da conta; nenhum dos dois é digitado na finalização.
- RN04: O preço, o nome e a foto de cada item vêm do catálogo no momento da finalização; os valores enviados pela loja são ignorados. O total é calculado pelo sistema (preço unitário × quantidade de cada item).
- RN05: A data do pedido é a data da finalização.
- RN06: O pedido só é confirmado após o pagamento aprovado (UC04).
- RN07: O registro do pedido não depende do serviço de pagamentos estar no ar; a cobrança é feita quando ele voltar.

## Limitações conhecidas

- Não há verificação de estoque.
- Se o preço mudou desde que o produto entrou no carrinho, vale o do catálogo; o cliente vê o total final só na confirmação.

## Pós-condição

Pedido registrado como **Pendente**, aguardando o pagamento.
