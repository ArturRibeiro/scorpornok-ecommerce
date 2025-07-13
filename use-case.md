
 ### 📄 Requisitos do caso de uso (simplificados)

### 📦 Cenário 1: O cliente pode adicionar produtos ao carrinho

📄 Etapas resumidas
- 1️⃣ O cliente começa a adicionar produtos ao carrinho de compras.
- 2️⃣ Quando decide finalizar, o carrinho se transforma em uma ordem de compra (Order).
- 3️⃣ A ordem é criada no sistema para persistir os dados (produtos, endereço, forma de pagamento, etc.).
- 4️⃣ Essa ordem só será efetivamente confirmada após o pagamento ser aprovado.

### 2 - O cliente pode remover produtos do carrinho.
### 3 - O cliente pode consultar o valor total.
### 4 - O carrinho mantém uma lista de itens, cada um com quantidade.


💡 Fluxo típico

```mermaid
[Handler]
↓
Validar command CreditCardPaymentCommand
↓
Recuperar Order no repositório (status Pending)
↓
Enviar dados ao Gateway de Pagamento
↓
Receber resposta (Aprovado / Recusado)
↓
Atualizar status da Order (Confirmed ou Failed)
↓
Salvar alterações
↓
Publicar eventos, se necessário
