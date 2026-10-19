Feature: Catalog
Simple calculator for adding two numbers

    Scenario: Listar todos os produtos em destaques
        Given QUE esteja na tela principal
        Then listo todos os produtos em destaques

    Scenario: Buscar vários produtos pelos ids
        When busco os produtos 1, 2 e 999999
        Then recebo só os produtos 1 e 2

    Scenario: Primeira página pelo GraphQL
        When consulto pelo GraphQL a página 1 com 18 produtos
        Then recebo 18 produtos e o total do catálogo
        And há página seguinte e não há anterior

    Scenario: Tamanho acima do limite pelo GraphQL
        When consulto pelo GraphQL a página 1 com 50 produtos
        Then recebo 20 produtos e o total do catálogo

    Scenario: Produto inexistente pelo GraphQL
        When consulto pelo GraphQL o produto 999999
        Then a resposta não tem produto nem erro

    Scenario: Mesma página no GraphQL e no REST
        When consulto a página 2 com 18 produtos pelo GraphQL e pelo REST
        Then as duas respostas trazem os mesmos produtos, na mesma ordem e com o mesmo preço
