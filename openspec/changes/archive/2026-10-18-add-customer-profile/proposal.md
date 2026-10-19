# Proposal

## Why

Hoje a conta do cliente tem só o e-mail: nome, telefone, CPF e endereço não ficam guardados em lugar nenhum, e o cliente digita o endereço de novo a cada compra. Um cadastro completo, preenchido depois do login, dá à loja os dados do cliente e agiliza o checkout.

## What Changes

- Novo bounded context **Customers**, com API e banco próprios, dono do cadastro do cliente.
- O cliente logado (e-mail confirmado) vê e edita o seu cadastro: nome completo, telefone, CPF e um endereço de entrega.
- O cadastro é ligado à conta do Keycloak; cada cliente só lê e altera o próprio cadastro.
- Todos os campos do cadastro são obrigatórios, exceto o complemento do endereço, e são validados (CPF com dígitos verificadores, telefone, CEP, UF).
- Nova página "Meu cadastro" na loja, acessível pelo menu da conta no cabeçalho.
- O checkout passa a pedir nome, telefone e CPF e abre com eles e o endereço preenchidos pelo cadastro; o que o cliente mudar vale só para aquele pedido.
- O endereço de entrega acompanha o pedido, pode ser diferente do cadastro e passa ao formato dele (com número, bairro e UF, no Brasil).
- Cada pedido registrado envia ao Customers, pelo RabbitMQ, os dados do comprador e o endereço de entrega: o endereço entra num histórico e, na primeira compra, os dados criam o cadastro; um cadastro existente não muda.

## Fora do escopo

- Vários endereços no cadastro, apelidos de endereço ou endereço de cobrança separado.
- Atualizar um cadastro existente com os dados da compra.
- Entrega fora do Brasil.
- Tela ou consulta do histórico de endereços.
- Troca de e-mail ou de senha.
- Busca de endereço pelo CEP.
- Guardar nome, telefone ou CPF no pedido, ou levá-los ao pagamento ou aos e-mails.
- Exigir o cadastro salvo para comprar.
- Exclusão da conta e exportação dos dados (LGPD).

## Capabilities

### New Capabilities
- `customer-profile`: cadastro do cliente logado (nome, telefone, CPF e endereço de entrega), com leitura e edição só pelo dono, validação dos campos, criação na primeira compra e histórico dos endereços de entrega dos pedidos.

### Modified Capabilities
- `checkout`: dados do comprador obrigatórios e endereço no formato do cadastro, preenchidos com os do cadastro, quando houver.
- `order-placement`: endereço no formato do cadastro, dados do comprador obrigatórios e envio das informações da compra ao cadastro.

## Impact

- Back-end: novo contexto Customers na solution; Orders com o endereço no novo formato (exige recriar o banco do Orders), dados do comprador e a mensagem da compra; regras de CPF, telefone, UF e CEP compartilhadas.
- Infra: novos serviços da API e do banco do Customers no compose, ligados ao RabbitMQ; realm do Keycloak com o audience da nova API.
- Front-end: página de cadastro, menu da conta e checkout com dados do comprador e novo endereço.
- Documentação: guia do projeto e arquivo de requisições da nova API.
