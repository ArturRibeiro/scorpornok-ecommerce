# Observabilidade

Como acompanhar o que acontece dentro da loja, de uma ponta à outra. Cada item indica a sua situação.

## 13. Acompanhar uma compra de ponta a ponta

**Problema:** uma compra passa pelo ponto de entrada, por quatro partes do sistema, pelo RabbitMQ e por tarefas em segundo plano. Quando algo dava errado, só havia o log de cada parte, separado, e era preciso juntá-los à mão pelo número do pedido.

**Solução:**
- Cada parte registra o que fez numa compra (as requisições, as chamadas às outras partes, o banco e as mensagens) num formato aberto. Uma compra aparece inteira, da loja até o resultado do pagamento, com os logs de cada passo ao lado, num painel local, sem conta. O formato aberto permite trocar o painel por outro serviço sem mudar o código.
- Cada compra é encontrada pelo número do pedido, e a lista mostra o que cada requisição fez (registrar o pedido, consultar, tokenizar o cartão), e não só o endereço chamado.
- Nada do cartão, do CPF, do telefone, do e-mail ou do token de acesso sai da loja na telemetria. Do GraphQL, vai só o tipo e o nome da operação, porque o cartão está no próprio texto da tokenização. Testes conferem isso a cada build.
- As verificações de saúde e as rodadas das tarefas em segundo plano sem trabalho não são enviadas, para não esconder as compras no meio do ruído.
- O painel é opcional: sem o endereço dele, nada é enviado, e com ele fora do ar a loja funciona igual.

**Situação:** em funcionamento, com o painel local: a compra inteira num trace só, encontrada pelo número do pedido, sem dados sensíveis e sem ruído.

Detalhes técnicos: `add-service-telemetry` (arquivada em `openspec/changes/archive/`).
