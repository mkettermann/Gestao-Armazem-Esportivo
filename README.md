# Gestão de Armazém Esportivo

Sistema de microserviços em .NET 9 para gestão de estoque de produtos esportivos, com autenticação JWT, mensageria assíncrona via RabbitMQ e observabilidade completa.

---

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (com Docker Compose)
- .NET 9 SDK (apenas para desenvolvimento local fora do Docker)
- [VS Code](https://code.visualstudio.com/) + extensão [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) *(opcional, para rodar os arquivos `.http`)*

---

## Executando o Sistema

### 1. Configure as variáveis de ambiente

Crie um arquivo `.env` na raiz do projeto:

```env
POSTGRES_USER=armazem
POSTGRES_PASSWORD=armazem@2024

RABBITMQ_USER=admin
RABBITMQ_PASSWORD=admin@2024

JWT_CHAVE=MinhaChaveSuperSecretaComMaisDe32Caracteres!

GRAFANA_PASSWORD=grafana@2024
```

### 2. Suba todos os serviços

```bash
docker-compose up --build -d
```

### 3. Aguarde a inicialização

Todos os serviços aguardam o PostgreSQL e o RabbitMQ ficarem saudáveis antes de subir.
As migrations do banco de dados são aplicadas automaticamente na inicialização.

---

## Serviços e Portas

| Serviço          | URL Local                    |
|------------------|------------------------------|
| **Gateway**      | <http://localhost:8080>        |
| Identidade API   | <http://localhost:5001>        |
| Catálogo API     | <http://localhost:5002>        |
| Estoque API      | <http://localhost:5003>        |
| Pedidos API      | <http://localhost:5004>        |

> Todas as chamadas devem ser feitas pelo Gateway na porta **8080**.

---

## Testando os Endpoints

Há duas formas de testar a API:

### Opção A — VS Code REST Client (recomendado)

Com o projeto aberto no VS Code e a extensão [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) instalada, abra qualquer arquivo da pasta `http/` e clique em **Send Request** acima de cada chamada.

| Arquivo | Conteúdo |
|---|---|
| `http/01-autenticacao.http` | Registrar usuário (Admin e Vendedor), Login |
| `http/02-catalogo.http` | Criar, listar, obter, atualizar e remover produtos |
| `http/03-estoque.http` | Adicionar entrada de estoque, consultar estoque |
| `http/04-pedidos.http` | Emitir pedido, consultar pedido por ID |

**Fluxo sugerido:**

1. Execute **"Registrar usuário Administrador"** em `01-autenticacao.http`
2. Execute **"Login - Administrador"** e copie o `token` retornado
3. Cole o token na variável `@tokenAdmin` nos arquivos `02`, `03` e `04`
4. Siga o fluxo completo (produto → estoque → pedido)

### Opção B — Seguir os exemplos neste README

Os exemplos completos estão na seção **Fluxo de Uso Completo** logo abaixo. Você pode copiá-los para qualquer cliente HTTP (curl, Insomnia, Postman etc.).

---

## Fluxo de Uso Completo

### 1. Registrar um usuário administrador

```http
POST http://localhost:8080/auth/usuarios
Content-Type: application/json

{
  "nome": "Admin",
  "email": "admin@armazem.com",
  "senha": "Senha@123",
  "tipoUsuario": "Administrador"
}
```

### 2. Fazer login e obter o token JWT

```http
POST http://localhost:8080/auth/login
Content-Type: application/json

{
  "email": "admin@armazem.com",
  "senha": "Senha@123"
}
```

Guarde o `token` retornado. Use-o no header `Authorization: Bearer <token>` em todas as próximas requisições.

### 3. Registrar um usuário Vendedor *(opcional)*

```http
POST http://localhost:8080/auth/usuarios
Content-Type: application/json

{
  "nome": "Vendedor Teste",
  "email": "vendedor@armazem.com",
  "senha": "Senha@123",
  "tipoUsuario": "Vendedor"
}
```

### 4. Cadastrar um produto

```http
POST http://localhost:8080/produtos
Authorization: Bearer <token>
Content-Type: application/json

{
  "nome": "Bola de Futebol",
  "descricao": "Bola oficial tamanho 5",
  "preco": 149.90
}
```

Guarde o `id` do produto retornado.

### 5. Listar produtos

```http
GET http://localhost:8080/produtos?pagina=1&tamanhoPagina=20
Authorization: Bearer <token>
```

### 6. Obter produto por ID

```http
GET http://localhost:8080/produtos/<id-do-produto>
Authorization: Bearer <token>
```

### 7. Atualizar produto

```http
PUT http://localhost:8080/produtos/<id-do-produto>
Authorization: Bearer <token>
Content-Type: application/json

{
  "nome": "Bola de Futebol Pro",
  "descricao": "Bola oficial tamanho 5, costura tripla reforçada",
  "preco": 199.90
}
```

### 8. Adicionar estoque ao produto

```http
POST http://localhost:8080/estoque/<id-do-produto>/entradas
Authorization: Bearer <token>
Content-Type: application/json

{
  "quantidade": 50,
  "numeroNotaFiscal": "NF-001/2024"
}
```

### 9. Consultar estoque

```http
GET http://localhost:8080/estoque/<id-do-produto>
Authorization: Bearer <token>
```

### 10. Criar um pedido

```http
POST http://localhost:8080/pedidos
Authorization: Bearer <token>
Content-Type: application/json

{
  "documentoCliente": "12345678901",
  "nomeVendedor": "Vendedor Silva",
  "itens": [
    {
      "produtoId": "<id-do-produto>",
      "quantidade": 3
    }
  ]
}
```

O pedido é confirmado, o estoque é debitado assincronamente via RabbitMQ e o evento `PedidoConfirmadoEvento` é publicado.

### 11. Consultar um pedido por ID

```http
GET http://localhost:8080/pedidos/<id-do-pedido>
Authorization: Bearer <token>
```

### 12. Remover produto

```http
DELETE http://localhost:8080/produtos/<id-do-produto>
Authorization: Bearer <token>
```

---

## Endpoints por Serviço

### Identidade (`/auth/**`)

| Método | Rota              | Descrição              | Auth |
|--------|-------------------|------------------------|------|
| POST   | /auth/usuarios    | Registrar usuário      | Não  |
| POST   | /auth/login       | Login e obter token    | Não  |

### Catálogo (`/produtos/**`)

| Método | Rota              | Descrição              | Auth          |
|--------|-------------------|------------------------|---------------|
| GET    | /produtos         | Listar produtos        | Sim           |
| GET    | /produtos/{id}    | Obter produto por ID   | Sim           |
| POST   | /produtos         | Criar produto          | Administrador |
| PUT    | /produtos/{id}    | Atualizar produto      | Administrador |
| DELETE | /produtos/{id}    | Desativar produto      | Administrador |

### Estoque (`/estoque/**`)

| Método | Rota                    | Descrição              | Auth          |
|--------|-------------------------|------------------------|---------------|
| GET    | /estoque/{produtoId}    | Consultar estoque      | Sim           |
| POST   | /estoque/{produtoId}/entradas | Registrar entrada | Administrador |

### Pedidos (`/pedidos/**`)

| Método | Rota              | Descrição              | Auth    |
|--------|-------------------|------------------------|---------|
| POST   | /pedidos          | Criar pedido           | Sim     |
| GET    | /pedidos/{id}     | Obter pedido por ID    | Sim     |

---

## Health Checks

Todos os serviços expõem `/health`:

```bash
curl http://localhost:8080/health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
```

---

## Observabilidade

| Ferramenta       | URL                          | Credenciais             |
|------------------|------------------------------|-------------------------|
| **Jaeger** (Traces) | <http://localhost:16686>    | -                       |
| **Prometheus** (Métricas) | <http://localhost:9090> | -                  |
| **Grafana** (Dashboards) | <http://localhost:3000>  | admin / `GRAFANA_PASSWORD` |
| **RabbitMQ Management** | <http://localhost:15672> | `RABBITMQ_USER` / `RABBITMQ_PASSWORD` |

Os traces são enviados via **OpenTelemetry Protocol (OTLP)** para o collector, que repassa ao Jaeger e ao Prometheus.

---

## Arquitetura

```
┌─────────────────────────────────────────────┐
│              Gateway (YARP :8080)            │
└──────┬──────────┬──────────┬────────────────┘
       │          │          │         │
  Identidade  Catálogo   Estoque   Pedidos
   (:5001)    (:5002)    (:5003)   (:5004)
       │          │          │         │
       └──────────┴────────── ┴─────────┘
                      │
               PostgreSQL (:5432)
                      │
               RabbitMQ (:5672)
```

Cada serviço segue **Clean Architecture** com camadas:

- **Api** — Controllers, Program.cs, Middlewares
- **Application** — Serviços de aplicação, DTOs, Clientes HTTP
- **Domain** — Entidades, Value Objects, Factories, Interfaces
- **Infrastructure** — EF Core DbContext, Repositórios, Mensageria

---

## Documentação Adicional

- [Descrição do Sistema](docs/Descricao.md)
