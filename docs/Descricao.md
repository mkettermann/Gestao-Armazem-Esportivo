# API para Gestão de Estoque e Produtos

- Sistema em .NET contendo a gestão de um armazenamento de produtos esportivos.

## Visão Geral

- Este projeto consiste no desenvolvimento de uma solução backend para controle de estoque e gerenciamento de um catálogo de produtos. A implementação de frontend não é necessária, mas a API deverá ser projetada para ser consumida por uma aplicação web cliente, que fornecerá essas funcionalidades como parte de um sistema de gestão. Portanto, é fundamental considerar aspectos como usabilidade, organização do código, arquitetura limpa, desempenho e tratativa de erros.

- O domínio de negócio da aplicação é equipamentos esportivos.
- O código deve ser organizado, claro e com boas práticas de desenvolvimento em um cenário que simula um ambiente de produção. Utilizar variáveis em português sem acentos e erros em português devidamente acentuados.

## Ao implementar, objetivo do projeto

- Aplicação de boas práticas de desenvolvimento e arquitetura
- Estrutura de código limpa, organizada e aplicando design pattern de factory para ter um padrão de uso na base.
- Funcionalidades bem implementadas e cada um com teste unitário
- Tratamento de erros e exceções com um retorno usando design pattern facade com mensagem, dados e sucesso.
- Clareza na documentação das decisões técnicas
- Utilizar variáveis em português sem acentos e erros em português devidamente acentuados

## Histórias de Usuário

### H1 – Cadastro de Usuários

- Como usuário do sistema
- Quero cadastrar meu usuário
- Para que eu possa ter acesso à plataforma
- Requisitos:
  - Selecionar tipo de usuário: Administrador ou Vendedor
  - O usuário deve possuir: nome, e-mail (único) e senha com no mínimo 6 caracteres

### H2 – Login

- Como usuário do sistema
- Quero realizar login
- Para que eu possa acessar o sistema conforme minhas permissões
- Requisitos:
  - Login deve ser feito via e-mail
  - O login deve retornar um token de autenticação

### H3 – Gerenciamento de Produtos

- Como administrador
- Quero cadastrar, editar, listar, consultar e excluir produtos
- Para que os vendedores possam visualizá-los e utilizá-los nos pedidos
- Requisitos:
  - O produto deve conter: nome, descrição e preço

### H4 – Controle de Estoque

- Como administrador
- Quero adicionar estoque a um produto existente
- Para que os vendedores saibam a quantidade disponível para venda
- Requisitos:
  - Informar a quantidade adquirida
  - Registrar o número da nota fiscal para fins de auditoria

### H5 – Emissão de Pedidos

- Como vendedor
- Quero gerar pedidos para meus clientes
- Para que eu possa registrar vendas e controlar o estoque automaticamente
- Requisitos:
  - Selecionar produtos cadastrados e informar a quantidade desejada
  - Informar o documento do cliente
  - Informar o nome do vendedor
  - Caso algum produto não possua estoque suficiente, o pedido não deve ser criado, e uma mensagem de erro deve ser exibida
  - Ao confirmar, o sistema deve dar baixa no estoque dos produtos incluídos

## Artefatos Esperados

- API .NET pronta para execução via Docker
- Testes unitários implementados
- Documentação clara das funcionalidades para fins de QA (README com instruções)
- Implementação de Tracing e APM
- Uso de microsserviços com mensageria RabbitMQ.

## Restrições

- O backend deve ser desenvolvido em .NET Core
- Você pode utilizar quaisquer bibliotecas ou ferramentas auxiliares, desde que devidamente documentadas

## Documentação

- Inclua um README.md com instruções claras para execução e testes e finalizar com os links para os demais documentos (.md)
- Queremos percorrer o fluxo completo da aplicação, portanto descreva os passos necessários para cadastrar usuários, produtos, estoque e realizar um pedido com sucesso
