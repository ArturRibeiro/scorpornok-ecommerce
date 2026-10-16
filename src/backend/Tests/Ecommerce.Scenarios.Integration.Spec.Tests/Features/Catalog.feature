Feature: Catalog
Simple calculator for adding two numbers

    Scenario: Listar todos os produtos em destaques
        Given QUE esteja na tela principal
        Then listo todos os produtos em destaques

    Scenario: Buscar vários produtos pelos ids
        When busco os produtos 1, 2 e 999999
        Then recebo só os produtos 1 e 2
