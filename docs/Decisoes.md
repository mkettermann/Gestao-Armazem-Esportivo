# Decisões Técnicas (ADR resumido)

Este documento registra as principais decisões de arquitetura e design do **Gestão de Armazém
Esportivo**, com o contexto e os trade-offs de cada uma.

---

## 1. Microserviços + Clean Architecture por serviço

Quatro serviços independentes (Identidade, Catálogo, Estoque, Pedidos), cada um com as camadas
**Domain → Application → Infrastructure → Api** e seu **próprio banco PostgreSQL** (database por
serviço). As dependências apontam para dentro (Domain não conhece infraestrutura).

- **Por quê:** isolamento de domínio, deploy independente, escalabilidade por serviço.
- **Trade-off:** não há transação distribuída entre serviços — tratado com saga/outbox (item 5).

## 2. Padrões de projeto

- **Factory** (`*Factory` no domínio): ponto único de criação das entidades, concentrando invariantes
  (ex.: senha ≥ 6, preço > 0, geração de Id, hash de senha).
- **Facade de resposta**: toda resposta HTTP usa o envelope `RespostaApi { sucesso, mensagem, dados }`
  e, internamente, `Resultado<T> { foiSucesso, valor, erro }`. Padroniza sucesso e erro.
- **Repository**: interfaces no domínio, implementação em Infrastructure (inversão de dependência).

## 3. Gateway: YARP

Ponto de entrada único (porta 8080) com roteamento por prefixo (`/auth`, `/produtos`, `/estoque`,
`/pedidos`). CORS é aplicado aqui, por ser a superfície consumida pela aplicação web cliente.

- **Por quê YARP:** nativo .NET, configuração declarativa, sem dependexterna de gateway.

## 4. Mensageria: RabbitMQ.Client puro + projeto compartilhado

Optou-se pelo **RabbitMQ.Client** diretamente (em vez de MassTransit) para deixar explícito o uso de
exchanges, filas, DLQ, confirmações de publicação e propagação de contexto. A infraestrutura comum
(conexão persistente, publicador com *publisher confirms*, consumidor base com DLQ/ACK/NACK e
propagação de trace) foi extraída para o projeto **`shared/Shared.Mensageria`**, evitando duplicação
entre serviços.

- **Trade-off:** mais código de baixo nível do que um framework, em troca de controle e transparência.

## 5. Consistência distribuída: Outbox + Saga + Idempotência (decisão central)

O fluxo Pedido → Estoque foi redesenhado para eliminar três defeitos do modelo anterior
(dual-write, dupla baixa por reentrega e venda além do disponível):

- **Transactional Outbox (Pedidos):** o pedido e o evento de integração são gravados na **mesma
  transação**; um processo em segundo plano publica o evento. Elimina o *dual-write*.
- **Saga (coreografia):** o pedido nasce **`Pendente`**. O Estoque dá baixa e responde
  `EstoqueBaixadoEvento` (→ `Confirmado`) ou `EstoqueRejeitadoEvento` (→ `Rejeitado`). Assim nenhum
  pedido fica `Confirmado` sem baixa efetiva — fecha a janela de *overselling*.
- **Consumidor idempotente (Estoque):** tabela `eventos_processados` (chave = `idEvento`) garante que
  uma reentrega não aplique a baixa duas vezes; a baixa de todos os itens + o registro de
  idempotência ocorrem em **uma transação**, verificando a disponibilidade de todos antes de aplicar
  (sem baixa parcial).
- **Concorrência otimista:** a coluna de sistema **`xmin`** do PostgreSQL é usada como token de
  concorrência em `itens_estoque`, evitando *lost update* entre baixas concorrentes.
- **Pré-validação síncrona:** ao emitir, ainda há checagem HTTP de estoque para dar **erro imediato**
  no caso comum (requisito H5); a baixa autoritativa é a assíncrona.

- **Trade-off:** **consistência eventual** — o pedido fica `Pendente` por um curto intervalo até a
  confirmação. Consulte o status em `GET /pedidos/{id}`.

## 6. Observabilidade (Tracing e APM)

**OpenTelemetry** exporta traces e métricas via OTLP para o **OTEL Collector** → **Jaeger** (traces)
e **Prometheus/Grafana** (métricas). Instrumentação de ASP.NET Core, HttpClient e **Npgsql** (banco).
O **contexto de trace é propagado pelos cabeçalhos das mensagens RabbitMQ** (W3C Trace Context), de
modo que um único trace atravessa Gateway → Pedidos → (fila) → Estoque sem quebra.

## 7. Resiliência

Clientes HTTP internos (Pedidos → Catálogo/Estoque) usam `AddStandardResilienceHandler`
(timeout, *retry* com *backoff* e *circuit breaker*), evitando que indisponibilidades momentâneas
derrubem a emissão de pedidos.

## 8. Segurança

JWT com papéis **Administrador**/**Vendedor** (autorização por endpoint), senhas com **BCrypt**, e
**fail-fast** se `JWT:Chave` ausente ou com menos de 32 caracteres. Em produção, os segredos vêm de
variáveis de ambiente (`.env`/orquestrador), não do `appsettings.json`.

## 9. Validação em duas camadas

- **FluentValidation** valida o formato da entrada (DTOs) e responde 400 no envelope `RespostaApi`.
- **Validação de domínio** (nas entidades/value objects) protege as invariantes de negócio
  independentemente da origem da chamada.

## 10. Idempotência de API

Endpoints de escrita aceitam o header **`Idempotency-Key`**; respostas são memorizadas para evitar
efeitos duplicados em caso de *retry* do cliente (complementa a idempotência do consumidor de eventos).
