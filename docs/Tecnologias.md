# Tecnologias Utilizadas

Este documento lista as tecnologias, frameworks, bibliotecas e ferramentas empregadas no
**Gestão de Armazém Esportivo**, agrupadas por finalidade. Versões marcadas com `*` (ex.: `9.*`)
seguem a faixa declarada nos arquivos `.csproj`; as demais são fixadas explicitamente.

---

## Plataforma e Linguagem

| Tecnologia | Versão | Uso |
|---|---|---|
| **.NET** | 9 (`net9.0`) | Runtime e SDK de todos os serviços |
| **C#** | 13 | Linguagem (com `Nullable` e `ImplicitUsings` habilitados) |

Padronização de build em [Directory.Build.props](../Directory.Build.props): `TreatWarningsAsErrors`
ativo (qualidade uniforme), com exceção apenas dos avisos informativos de auditoria do NuGet
(`NU1901`–`NU1904`).

---

## API e Web

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| **ASP.NET Core** | 9 (`Microsoft.NET.Sdk.Web`) | Hospedagem das APIs e do Gateway |
| `Microsoft.AspNetCore.OpenApi` | 9.0.15 | Geração de OpenAPI |
| `Swashbuckle.AspNetCore` | 7.* | Swagger UI (com botão **Authorize** para JWT) |
| `Asp.Versioning.Mvc` | 8.* | Versionamento de API |

---

## Gateway (Proxy Reverso)

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| **YARP** (`Yarp.ReverseProxy`) | 2.* | Proxy reverso e ponto de entrada único (porta 8080) |

Detalhes de roteamento em [Gateway.md](Gateway.md).

---

## Persistência

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| **PostgreSQL** | 16 (`postgres:16-alpine`) | Banco relacional — um *database* por serviço |
| **Entity Framework Core** | 9.0.17 | ORM e mapeamento das entidades |
| `Npgsql` | 9.* | Driver PostgreSQL para .NET |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.1 | Provedor EF Core para PostgreSQL |
| `Microsoft.EntityFrameworkCore.Design` / `.Tools` | 9.* | Migrations (aplicadas na inicialização) |

A coluna de sistema **`xmin`** do PostgreSQL é usada como token de concorrência otimista no estoque.

---

## Mensageria

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| **RabbitMQ** | 3.13 (`rabbitmq:3.13-management-alpine`) | Broker de mensagens (saga, outbox, DLQ) |
| `RabbitMQ.Client` | 7.* | Cliente AMQP puro (sem framework de abstração) |

A infraestrutura comum (conexão persistente, *publisher confirms*, consumidor base com DLQ/ACK/NACK
e propagação de trace) é centralizada no projeto **`shared/Shared.Mensageria`**. O fluxo da saga está
descrito em [Comunicacao.md](Comunicacao.md).

---

## Segurança e Autenticação

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| **JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`) | 9 | Autenticação por token (papéis Administrador/Vendedor) |
| `BCrypt.Net-Next` | 4.* | Hash de senhas |

---

## Validação

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| `FluentValidation` | 11.* | Validação de formato de entrada (DTOs) → resposta 400 no envelope `RespostaApi` |

Complementada pela **validação de domínio** nas entidades/value objects (invariantes de negócio).

---

## Resiliência

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| `Microsoft.Extensions.Http.Resilience` | 9.* | Timeout, *retry* com *backoff* e *circuit breaker* nos clientes HTTP internos |

---

## Observabilidade (Tracing, Métricas e APM)

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| **OpenTelemetry** (`Exporter.OpenTelemetryProtocol`, `Extensions.Hosting`) | 1.* | Exportação de traces e métricas via OTLP |
| `OpenTelemetry.Instrumentation.AspNetCore` / `.Http` / `.Runtime` | 1.* | Instrumentação automática de requisições, HttpClient e runtime |
| `OpenTelemetry.Instrumentation.Process` | 0.5.*-beta | Métricas de processo |
| **OTEL Collector** (`otel/opentelemetry-collector-contrib`) | latest | Coleta e repasse de telemetria |
| **Jaeger** (`jaegertracing/all-in-one`) | latest | Visualização de traces distribuídos |
| **Prometheus** (`prom/prometheus`) | latest | Armazenamento de métricas |
| **Grafana** (`grafana/grafana`) | latest | Dashboards |
| `AspNetCore.HealthChecks.NpgSql` | 9.0.0 | Health check de banco (`/health`) |
| `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | 9.* | Health check via EF Core |

O contexto de trace é propagado pelos cabeçalhos das mensagens RabbitMQ (W3C Trace Context), de modo
que um único trace atravessa Gateway → Pedidos → (fila) → Estoque sem quebra.

---

## Testes

| Pacote / Tecnologia | Versão | Uso |
|---|---|---|
| **xUnit** | 2.9.2 | Framework de testes |
| `FluentAssertions` | 6.* | Asserções legíveis |
| `Moq` | 4.* | *Mocks* e *stubs* |
| `Microsoft.NET.Test.Sdk` | 17.12.0 | Infraestrutura de execução de testes |
| `coverlet.collector` | 6.0.2 | Cobertura de código |
| `xunit.runner.visualstudio` | 2.8.2 | Integração com o runner do Visual Studio / `dotnet test` |

Há projetos de **teste unitário** por serviço e um projeto de **testes de integração**
(`tests/Integracao.Tests`).

---

## Containerização e Infraestrutura

| Tecnologia | Uso |
|---|---|
| **Docker** / **Docker Compose** | Empacotamento e orquestração local de todos os serviços e dependências |
| **Variáveis de ambiente** (`.env`) | Configuração de segredos e *connection strings* (sem segredos em `appsettings.json`) |

---

## Ferramentas de Desenvolvimento

| Ferramenta | Uso |
|---|---|
| **Visual Studio Code** | Editor de código |
| **REST Client** (extensão do VS Code) | Execução dos arquivos `.http` em `http/` |

---

## Assistentes de IA na Geração de Código

O desenvolvimento foi apoiado por assistentes de IA, usados como **auxiliares na geração e revisão de
código** — sempre com revisão humana das sugestões antes da incorporação:

| Ferramenta | Uso |
|---|---|
| **Claude Code** (Anthropic) | Assistente de IA via CLI/IDE para geração de código, refatorações, documentação e apoio à arquitetura |
| **GitHub Copilot** | Autocompletar e sugestões de código em linha durante a edição |

> Todas as decisões técnicas, padrões e trade-offs documentados em [Decisoes.md](Decisoes.md) foram
> definidos e validados pela equipe; as ferramentas de IA aceleraram a escrita, mas não substituíram a
> revisão e a responsabilidade humana sobre o código.
