Você é um arquiteto de software sênior especializado em .NET, microsserviços e desenvolvimento orientado a domínio. Sua tarefa é elaborar um plano de implementação completo, detalhado e estruturado para a seguinte especificação de projeto.

## Especificação do projeto

# API para Gestão de Estoque e Produtos

**Domínio:** Equipamentos esportivos
**Stack:** .NET Core (backend), Docker, RabbitMQ (mensageria), APM/Tracing
**Convenções de código:** variáveis em português sem acentos; mensagens de erro em português com acentuação correta.

### Histórias de usuário

- **H1 – Cadastro de usuários:** nome, e-mail único, senha ≥ 6 chars; tipos: Administrador | Vendedor
- **H2 – Login:** via e-mail; retorna token de autenticação (JWT)
- **H3 – Gerenciamento de produtos (Admin):** CRUD completo; campos: nome, descrição, preço
- **H4 – Controle de estoque (Admin):** adicionar quantidade + número de nota fiscal por produto
- **H5 – Emissão de pedidos (Vendedor):** selecionar produtos + qtd; informar documento do cliente e nome do vendedor; validar estoque suficiente antes de confirmar; dar baixa automática no estoque ao confirmar

---

## Sua tarefa

Gere o **plano de implementação completo** cobrindo obrigatoriamente os 12 blocos listados abaixo.

> ⚠️ **Modo de entrega:** Gere **um bloco por vez**, exibindo o conteúdo completo daquele bloco e então aguardando minha confirmação ("ok", "próximo", "continuar" ou similar) antes de avançar para o seguinte. Não antecipe blocos futuros.

> 📄 **Documento final:** Ao concluir todos os 12 blocos, consolide o plano inteiro em um único arquivo **`PLANO_IMPLEMENTACAO.md`** para download, contendo todos os blocos na ordem, com índice no topo e links âncora para cada seção.

---

### BLOCO 1 — Visão geral da arquitetura

- Diagrama textual (ASCII ou lista hierárquica) da estrutura de microsserviços
- Quais serviços existem, quais se comunicam via HTTP e quais via RabbitMQ
- Justificativa de separação de responsabilidades entre serviços
- Como o API Gateway (se houver) se integra ao conjunto

---

### BLOCO 2 — Estrutura de pastas e projetos

- Estrutura completa de diretórios do repositório (monorepo ou multi-repo, justificado)
- Camadas internas de cada serviço: quais projetos .csproj existem (ex: Domain, Application, Infrastructure, API)
- Onde ficam: entidades, repositórios, serviços de aplicação, controllers, DTOs, testes

---

### BLOCO 3 — Modelagem de domínio

- Entidades e seus campos com tipos C# explícitos, em português sem acentos
- Enums (ex: TipoUsuario), Value Objects relevantes
- Regras de negócio embutidas nas entidades (ex: validação de estoque em Pedido)
- Relacionamentos entre entidades (cardinalidade explícita)

---

### BLOCO 4 — Design patterns aplicados

Para cada pattern abaixo, especifique:

- Onde exatamente será aplicado (classe, namespace, camada)
- Código-esqueleto em C# ilustrando a implementação

Patterns obrigatórios:

1. **Factory** — padrão de criação para entidades/objetos de domínio
2. **Facade** — padronização de resposta da API (campos: `sucesso`, `mensagem`, `dados`)
3. **Repository** — abstração de acesso a dados
4. Qualquer outro pattern que você julgue necessário; justifique

---

### BLOCO 5 — Endpoints da API

Para cada história de usuário (H1–H5), liste:

- Método HTTP + rota
- Payload de request (JSON com tipos)
- Payload de response (JSON com tipos, usando o Facade)
- Código de status HTTP em caso de sucesso e de cada erro mapeado
- Quais roles têm acesso (Administrador | Vendedor | Público)

---

### BLOCO 6 — Mensageria com RabbitMQ

- Quais eventos são publicados e por qual serviço
- Quais serviços consomem cada evento e qual ação executam
- Nomeação de exchanges e queues (convenção explícita)
- Estratégia de tratamento de falhas: dead-letter queue, retry policy, idempotência

---

### BLOCO 7 — Autenticação e autorização

- Estratégia JWT: geração, claims incluídos no token (roles, identificadores)
- Middleware ou atributos de autorização no .NET
- Como o role `Administrador` vs `Vendedor` é aplicado por endpoint

---

### BLOCO 8 — Tracing e APM

- Qual biblioteca/stack será usada (ex: OpenTelemetry + Jaeger, ou outro)
- O que será instrumentado: HTTP requests, DB queries, publicação/consumo de mensagens
- Como configurar e visualizar (serviço de coleta, dashboard)
- Estrutura de configuração no `appsettings.json` ou variáveis de ambiente

---

### BLOCO 9 — Testes unitários

- Quais classes/serviços terão testes (lista explícita por história de usuário)
- Framework e bibliotecas de mock (xUnit, Moq, FluentAssertions, etc.)
- Exemplo de teste unitário para pelo menos dois cenários críticos: estoque insuficiente (H5) e criação de pedido com sucesso (H5)
- Estrutura de nomenclatura dos testes: convenção adotada

---

### BLOCO 10 — Docker e infraestrutura

- `docker-compose.yml` completo com todos os serviços: API(s), banco de dados, RabbitMQ, collector de APM
- Variáveis de ambiente necessárias por serviço
- Health checks e dependências entre serviços (`depends_on` com condição)
- Como fazer o build e subir o ambiente com um único comando

---

### BLOCO 11 — README e documentação

- Estrutura do README.md: seções obrigatórias
- Fluxo ponta a ponta descrito em passos numerados: cadastro de usuário → login → cadastro de produto → adição de estoque → emissão de pedido com sucesso
- Quais outros arquivos `.md` de documentação serão criados e sobre o quê
- Orientações para QA: como verificar cada história de usuário via curl ou Swagger

---

### BLOCO 12 — Ordem de implementação sugerida

- Sprint ou sequência de tarefas priorizadas
- Dependências entre tarefas (o que deve ser feito antes do quê)
- Estimativa relativa de esforço por bloco (P/M/G)

---

## Restrições e convenções a respeitar em todo o plano

- Nomes de variáveis, campos de entidade e parâmetros: **português sem acentos** (ex: `nomeVendedor`, `quantidadeDisponivel`)
- Mensagens de erro e descrições para o usuário final: **português com acentuação correta**
- Toda resposta de API usa o Facade com campos `sucesso` (bool), `mensagem` (string) e `dados` (object|null)
- Cada história de usuário deve ter pelo menos um teste unitário correspondente
- O projeto deve ser executável com `docker compose up` sem configuração manual adicional

---

Ao finalizar o Bloco 12, apresente um **índice rápido** com uma frase-resumo de cada bloco — e então gere o arquivo `PLANO_IMPLEMENTACAO.md` consolidado com todo o conteúdo.
