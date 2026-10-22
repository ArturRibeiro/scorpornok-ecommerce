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

    Scenario: Busca com palavras sem acento e fora de ordem
        When busco pelo GraphQL "revolution tenis nike"
        Then os resultados trazem o produto "Tênis Nike Revolution 8 Masculino"
        And o primeiro resultado tem "nike" e "revolution" no nome

    Scenario: Palavra que só está na descrição
        When busco pelo GraphQL "caminhada"
        Then recebo todos os produtos que citam "caminhada" no nome ou na descrição

    Scenario: Palavras em campos diferentes
        When busco pelo GraphQL "asics caminhada"
        Then os resultados trazem o produto "Tênis Asics Raiden 5 Masculino"

    Scenario: Nome antes da descrição
        When busco pelo GraphQL todas as páginas de "masculino"
        Then os produtos com "masculino" no nome vêm antes dos que só o citam na descrição

    Scenario: Sugestão pela descrição
        When busco pelo GraphQL as 5 sugestões de "a malha"
        Then as sugestões trazem produtos que citam "malha" na descrição

    Scenario: Curinga no termo vale como texto
        When busco pelo GraphQL "%"
        Then recebo todos os produtos que citam "%" no nome ou na descrição

    Scenario: Duas marcas sem termo
        When busco pelo GraphQL as marcas "Nike" e "Adidas"
        Then recebo todos os produtos dessas marcas, e só eles

    Scenario: Faixa de preço com os limites incluídos
        When busco pelo GraphQL os produtos de 399.99 a 500
        Then recebo todos os produtos nessa faixa, inclusive os de 399.99

    Scenario: Faixa de preço invertida
        When busco pelo GraphQL os produtos de 500 a 200
        Then nenhum produto é encontrado

    Scenario: Busca acima do limite por página
        When busco pelo GraphQL sem filtros com 50 por página
        Then a página da busca traz 20 produtos

    Scenario: Marcas do catálogo
        When consulto as marcas pelo GraphQL
        Then recebo as marcas do catálogo, sem repetição e em ordem alfabética
