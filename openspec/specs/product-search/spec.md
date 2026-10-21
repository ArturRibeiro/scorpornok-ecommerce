# product-search Specification

## Purpose

Busca de produtos da loja: pelo nome, pela marca e pela descrição, com sugestões enquanto o cliente digita, filtros por marca e faixa de preço e uma página de resultados paginada.

## Requirements

### Requirement: Busca por nome, marca e descrição
A busca DEVE encontrar os produtos em que cada palavra do termo aparece no nome, na marca ou na descrição, em qualquer ordem, sem diferenciar maiúsculas, minúsculas e acentos. Ela NÃO DEVE procurar no SKU.

#### Scenario: Palavras sem acento e fora de ordem
- **GIVEN** o catálogo tem o "Tênis Nike Revolution 8 Masculino"
- **WHEN** o cliente busca "revolution tenis nike"
- **THEN** o produto aparece nos resultados

#### Scenario: Palavra que só está na descrição
- **GIVEN** a palavra "caminhada" aparece só na descrição de alguns produtos
- **WHEN** o cliente busca "caminhada"
- **THEN** esses produtos aparecem nos resultados

#### Scenario: Palavras em campos diferentes
- **GIVEN** um produto da Asics cita "caminhada" só na descrição
- **WHEN** o cliente busca "asics caminhada"
- **THEN** o produto aparece nos resultados

### Requirement: Ordem dos resultados
Os resultados DEVEM vir primeiro com os produtos que têm todas as palavras no nome, depois os que precisam da marca e por último os que só são encontrados com a descrição. Dentro de cada grupo, DEVE valer a ordem do catálogo.

#### Scenario: Nome antes da descrição
- **GIVEN** alguns produtos têm "masculino" no nome e outros só na descrição
- **WHEN** o cliente busca "masculino"
- **THEN** todos os que têm a palavra no nome vêm antes dos que só a citam na descrição

### Requirement: Sugestões ao digitar
A partir de 2 caracteres, o campo de busca DEVE sugerir os 5 primeiros produtos da mesma busca da página de resultados, com foto, nome e preço, atualizados enquanto o cliente digita. Escolher uma sugestão DEVE abrir a página do produto, e confirmar o termo DEVE abrir a página de resultados.

#### Scenario: Sugestões
- **GIVEN** há mais de 5 produtos da Nike
- **WHEN** o cliente digita "nike"
- **THEN** aparecem 5 sugestões e a opção de ver todos os resultados

#### Scenario: Termo curto
- **GIVEN** o campo de busca está vazio
- **WHEN** o cliente digita "n"
- **THEN** nenhuma sugestão é mostrada

#### Scenario: Sugestão pela descrição
- **GIVEN** a palavra "malha" aparece só na descrição de alguns produtos
- **WHEN** o cliente digita "a malha"
- **THEN** esses produtos aparecem nas sugestões

#### Scenario: Sem sugestão
- **GIVEN** nenhum produto tem "xyz" no nome, na marca ou na descrição
- **WHEN** o cliente digita "xyz"
- **THEN** o campo informa que nenhum produto foi encontrado

### Requirement: Página de resultados
A página de resultados DEVE mostrar os produtos encontrados com a mesma grade e a mesma paginação da home, na ordem do requisito Ordem dos resultados, e informar quantos foram encontrados. O termo, os filtros e a página DEVEM ficar no endereço, e abrir esse endereço DEVE mostrar os mesmos resultados.

#### Scenario: Endereço compartilhado
- **GIVEN** o cliente buscou "nike" com o filtro até R$ 500 e está na página 2
- **WHEN** outra pessoa abre o mesmo endereço
- **THEN** vê o mesmo termo, o mesmo filtro e a mesma página de resultados

### Requirement: Filtro por marca
A página de resultados DEVE listar as marcas do catálogo e permitir escolher uma ou mais. Com marcas escolhidas, só os produtos de uma delas DEVEM aparecer, combinados com o termo e a faixa de preço.

#### Scenario: Duas marcas
- **GIVEN** o catálogo tem produtos da Nike, da Adidas e da Asics
- **WHEN** o cliente escolhe Nike e Adidas, sem termo
- **THEN** aparecem só os produtos da Nike e da Adidas

### Requirement: Filtro por faixa de preço
A página de resultados DEVE permitir informar um preço mínimo, um máximo ou ambos, com os limites incluídos. Com o mínimo maior que o máximo, a loja DEVE indicar a faixa como inválida, sem buscar.

#### Scenario: Limites incluídos
- **GIVEN** há um produto de R$ 399,99
- **WHEN** o cliente filtra de R$ 399,99 a R$ 500,00
- **THEN** o produto aparece nos resultados

#### Scenario: Faixa invertida
- **GIVEN** a página de resultados está aberta
- **WHEN** o cliente informa mínimo R$ 500 e máximo R$ 200
- **THEN** a faixa é indicada como inválida e os resultados não mudam

### Requirement: Nenhum resultado
Uma busca sem resultado DEVE informar que nenhum produto foi encontrado e oferecer limpar os filtros e voltar à loja.

#### Scenario: Filtros sem produto
- **GIVEN** nenhum produto da Skechers custa até R$ 200
- **WHEN** o cliente filtra Skechers até R$ 200
- **THEN** a página informa que nenhum produto foi encontrado e oferece limpar os filtros

### Requirement: Busca sem login e catálogo fora
A busca e as sugestões NÃO DEVEM exigir login. Com o catálogo fora do ar, a página de resultados DEVE oferecer tentar de novo, e as sugestões DEVEM sumir sem mensagem de erro.

#### Scenario: Catálogo fora
- **GIVEN** o catálogo está fora do ar
- **WHEN** o cliente busca "nike"
- **THEN** a página informa que não foi possível carregar os produtos e oferece tentar de novo
