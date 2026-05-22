# Discovery Heuristics

Patterns for detecting services, messages, channels, domains, and containers across languages and frameworks. Use these to form the internal draft map in Phase 2 of the skill.

Each section is organized as: **what to look for** → **where to find it** → **what to classify it as**.

## Table of contents

- [Project structure](#project-structure)
- [Service boundaries](#service-boundaries)
- [Messages: Node.js / TypeScript](#messages-nodejs--typescript)
- [Messages: Python](#messages-python)
- [Messages: Go](#messages-go)
- [Messages: Java / Kotlin / JVM](#messages-java--kotlin--jvm)
- [Messages: .NET](#messages-net)
- [Schema files](#schema-files)
- [Channels](#channels)
- [Containers](#containers)
- [Domains](#domains)
- [Classification rules](#classification-rules)

## Project structure

| Signal                                                      | What it means                       |
| ----------------------------------------------------------- | ----------------------------------- |
| `pnpm-workspace.yaml` / `package.json` with `workspaces`    | Monorepo (pnpm/yarn/npm workspaces) |
| `nx.json`, `turbo.json`, `lerna.json`                       | Monorepo with a tooling layer       |
| `go.work`                                                   | Go multi-module workspace           |
| Multiple top-level dirs each with their own manifest        | Polyglot monorepo                   |
| Single `package.json` / `pyproject.toml` / `go.mod` at root | Single-service repo                 |
| `Dockerfile` at root with one entrypoint                    | Single service                      |
| Multiple `Dockerfile`s under `services/*`                   | One service per Dockerfile          |

**Strategy:** read workspace config first, then fall back to directory structure + manifest scanning.

## Service boundaries

A service is an independently-deployable, independently-ownable unit. Positive signals (the more present, the stronger):

- Own manifest (`package.json`, `pyproject.toml`, `go.mod`, `pom.xml`, `*.csproj`)
- Own `Dockerfile` or deployment manifest (Helm chart, `serverless.yml`, CDK stack, k8s manifest)
- Own entrypoint (`main.ts`, `server.ts`, `main.go`, `app.py`, `Program.cs`, `Application.java`)
- Exposed over a network boundary (HTTP server, gRPC server, message consumer)
- Listed in `CODEOWNERS` as separately owned

**Negative signals** (probably not a service, more likely infrastructure):

- `lib/`, `shared/`, `common/`, `utils/` packages — usually code reuse, not a service
- No entrypoint — just exported functions/types
- No Dockerfile, no deployment config

**Edge cases:**

- Lambda / serverless handlers: a group of handlers under one service boundary often counts as one service, not one service per handler. Look at the deployment config.
- "Sidecar" services (auth-proxy, logger): list them but confirm with the user — they may or may not belong in the catalog.

## Messages: Node.js / TypeScript

### Message-bus clients

| Client            | Import pattern                                              | What to look for                                                                                     |
| ----------------- | ----------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| Kafka             | `kafkajs`, `@confluentinc/kafka-javascript`, `node-rdkafka` | `producer.send({ topic, messages })`, `consumer.subscribe({ topic })`                                |
| RabbitMQ          | `amqplib`, `amqp-connection-manager`                        | `channel.publish(exchange, routingKey, ...)`, `channel.consume(queue, ...)`                          |
| NATS              | `nats`, `nats.ws`                                           | `nc.publish(subject, ...)`, `nc.subscribe(subject, ...)`                                             |
| AWS SNS           | `@aws-sdk/client-sns`                                       | `new PublishCommand({ TopicArn, Message })`                                                          |
| AWS SQS           | `@aws-sdk/client-sqs`                                       | `new SendMessageCommand({ QueueUrl, MessageBody })`                                                  |
| AWS EventBridge   | `@aws-sdk/client-eventbridge`                               | `new PutEventsCommand({ Entries })` — each entry has `DetailType` and `Source` (strong event signal) |
| GCP PubSub        | `@google-cloud/pubsub`                                      | `topic.publishMessage(...)`, `subscription.on('message', ...)`                                       |
| Azure Service Bus | `@azure/service-bus`                                        | `sender.sendMessages(...)`                                                                           |
| MQTT              | `mqtt`, `async-mqtt`                                        | `client.publish(topic, ...)`                                                                         |

### Event emitter / in-process patterns

- `EventEmitter` usage — these are usually internal, not catalog-worthy. Skip unless the project clearly treats them as domain events.
- `nestjs/cqrs` — `@EventsHandler()`, `@CommandHandler()`, `@QueryHandler()` — **very strong** explicit classification, use these directly.
- Framework-level event buses (`tsed`, `midwayjs`): check project docs.

### DTO / type definitions

Look for:

- `interface OrderCreated { ... }` / `type OrderCreated = { ... }` — named like a message, payload-shaped.
- `zod` / `io-ts` / `typebox` schemas: `const OrderCreatedSchema = z.object({ ... })`.
- Class-validator DTOs (`class OrderCreatedDto { ... }`).

Pair these with a usage site (producer or consumer) before treating as a message.

## Messages: Python

### Message-bus clients

| Client   | Import                                 | Usage                                                                                          |
| -------- | -------------------------------------- | ---------------------------------------------------------------------------------------------- |
| Kafka    | `kafka`, `confluent_kafka`, `aiokafka` | `producer.send(topic, ...)`, `consumer.subscribe([topic])`                                     |
| RabbitMQ | `pika`, `aio_pika`                     | `channel.basic_publish(...)`, `channel.basic_consume(...)`                                     |
| Celery   | `celery`                               | `@app.task` — tasks are often commands. `task.delay(...)` / `task.apply_async(...)` is a send. |
| AWS      | `boto3`                                | `sns.publish(TopicArn=...)`, `sqs.send_message(QueueUrl=...)`, `events.put_events(...)`        |
| NATS     | `nats-py`                              | `nc.publish(subject, ...)`                                                                     |

### Event / message patterns

- Pydantic models named after events: `class OrderCreated(BaseModel): ...`
- Dataclasses named after events: `@dataclass class OrderCreated: ...`
- FastAPI background tasks (usually commands, not catalog-worthy unless crossing services)
- Django signals (`dispatch.Signal()`) — usually in-process, skip unless clearly cross-service

## Messages: Go

### Message-bus clients

| Client    | Import                                                                                                     | Usage                                                    |
| --------- | ---------------------------------------------------------------------------------------------------------- | -------------------------------------------------------- |
| Kafka     | `github.com/segmentio/kafka-go`, `github.com/Shopify/sarama`, `github.com/confluentinc/confluent-kafka-go` | `writer.WriteMessages(...)`, `reader.ReadMessage(...)`   |
| RabbitMQ  | `github.com/rabbitmq/amqp091-go`, `github.com/streadway/amqp`                                              | `channel.Publish(...)`, `channel.Consume(...)`           |
| NATS      | `github.com/nats-io/nats.go`                                                                               | `nc.Publish(...)`, `nc.Subscribe(...)`                   |
| AWS       | `github.com/aws/aws-sdk-go-v2/service/{sns,sqs,eventbridge}`                                               | Equivalent `Publish` / `SendMessage` / `PutEvents` calls |
| Watermill | `github.com/ThreeDotsLabs/watermill`                                                                       | Explicit pub/sub abstraction — strong signal             |

### Message patterns

- Structs named after events: `type OrderCreated struct { ... }`
- Topic name constants: `const OrderTopic = "orders.events"` — strong channel signal
- Handler funcs: `func HandleOrderCreated(ctx context.Context, msg OrderCreated) error` — classify based on return expectation and caller

## Messages: Java / Kotlin / JVM

### Message-bus clients

| Client              | Import                                         | Usage                                                                   |
| ------------------- | ---------------------------------------------- | ----------------------------------------------------------------------- |
| Kafka               | `org.apache.kafka:kafka-clients`, Spring Kafka | `KafkaTemplate.send(...)`, `@KafkaListener(topics = "...")`             |
| RabbitMQ            | Spring AMQP, `com.rabbitmq:amqp-client`        | `RabbitTemplate.convertAndSend(...)`, `@RabbitListener(queues = "...")` |
| AWS                 | AWS SDK v2                                     | `SnsClient.publish(...)`, `SqsClient.sendMessage(...)`                  |
| Spring Cloud Stream | `@EnableBinding`, `@StreamListener`            | Strong binding signal                                                   |

### Message patterns

- Spring `@EventListener`, `@TransactionalEventListener` — in-process events, usually skip
- Classes annotated `@Event`, `@Command` (Axon Framework) — explicit classification, use directly
- Records / data classes named after events (`record OrderCreated(...)`, `data class OrderCreated`)

## Messages: .NET

### Message-bus clients

| Client            | Namespace                    | Usage                                                                                        |
| ----------------- | ---------------------------- | -------------------------------------------------------------------------------------------- |
| MassTransit       | `MassTransit`                | `IPublishEndpoint.Publish<T>(...)`, `IConsumer<T>` — **very strong** explicit classification |
| NServiceBus       | `NServiceBus`                | `IMessageSession.Publish(...)`, `IHandleMessages<T>`                                         |
| Azure Service Bus | `Azure.Messaging.ServiceBus` | `ServiceBusSender.SendMessageAsync(...)`                                                     |
| Confluent Kafka   | `Confluent.Kafka`            | `producer.ProduceAsync(...)`, `consumer.Consume(...)`                                        |
| MediatR           | `MediatR`                    | `IRequest<T>` (query/command), `INotification` (event) — explicit by interface               |

### Message patterns

- Records named after events: `public record OrderCreated(...)`
- Message contracts in a `Messages/` or `Contracts/` project

## Schema files

File extensions that signal message contracts:

| Extension                                 | Format      | Meaning                                                |
| ----------------------------------------- | ----------- | ------------------------------------------------------ |
| `.schema.json`, `*.json` under `schemas/` | JSON Schema | Message payload contract                               |
| `.avsc`                                   | Avro        | Kafka-ecosystem message contract                       |
| `.proto`                                  | Protobuf    | gRPC methods (often queries/commands) or event schemas |
| `openapi.yaml`, `swagger.json`            | OpenAPI     | HTTP API — queries/commands via REST                   |
| `*.graphql`, `schema.graphql`             | GraphQL     | Queries / mutations / subscriptions                    |

Schema files are **strong evidence**. If a schema has the same name as a detected message, use the schema as the message's payload contract and note its format for `catalog-documentation-creator`.

## Channels

A channel is any named piece of messaging infrastructure. Look for:

- **Kafka topics** — string literals passed to producers/consumers. Group them: `orders.events`, `orders.commands`, `payments.events`.
- **RabbitMQ queues/exchanges** — `channel.assertQueue('name')`, `channel.assertExchange('name', 'topic')`.
- **SNS topic ARNs** — `arn:aws:sns:region:account:name`.
- **SQS queue URLs** — `https://sqs.region.amazonaws.com/account/name`.
- **EventBridge bus names** — `new PutEventsCommand({ Entries: [{ EventBusName, ... }] })`.
- **HTTP endpoints for query services** — `GET /users/:id`, `GET /orders/:id`.
- **gRPC services** — `service OrderService { rpc GetOrder(...) returns (...); }`.

Look in config files too (`config/kafka.yaml`, `appsettings.json`, env var defaults) — sometimes the topic name is only there.

## Containers

Databases, caches, and stores referenced in the code. Evidence:

- **Connection string parsing** — `postgres://`, `mysql://`, `mongodb://`, `redis://`
- **Env var names** — `DATABASE_URL`, `POSTGRES_HOST`, `REDIS_URL`, `DYNAMODB_TABLE`
- **Client instantiation** — `new PrismaClient()`, `new MongoClient(...)`, `createClient({ url: 'redis://...' })`, `DynamoDBClient`
- **Migrations / schema** — `prisma/schema.prisma`, `migrations/` dir, `alembic/versions/`
- **ORM models** — a project with ORM models pointing at one DB usually has one container per service

Tag each container with the service that uses it, and a type (`postgres`, `redis`, `dynamodb`, `s3`, etc.).

## Domains

Domains are the hardest to detect mechanically. Good signals:

- **Top-level code grouping** — `src/orders/`, `src/payments/`, `src/shipping/` — each is a candidate domain.
- **Package namespaces** — `com.company.orders.*`, `com.company.payments.*` in Java/Kotlin.
- **Bounded-context language in READMEs** — "the Orders domain", "Payments bounded context".
- **`CODEOWNERS`** — teams listed against directory groups often correlate with domains.
- **Monorepo workspace names** — `@company/orders-service`, `@company/payments-service`.

**When to propose domains:** only if you have ≥2 clear clusters. Otherwise propose "single domain for the whole codebase" and let the user split in grilling.

**Do not:**

- Invent domain names the user has never used. If you see `src/orders/`, call the domain `Orders` (match the folder name).
- Collapse services into a single domain just because they share code — shared code is infrastructure.

## Classification rules

For every message candidate, apply these rules to classify as **event**, **command**, **query**, or **uncertain**.

### Event indicators

- Past-tense name (`OrderCreated`, `PaymentProcessed`, `UserRegistered`)
- Published to a topic/exchange with multiple potential subscribers
- Producer does not wait for a reply
- Sent via pub/sub (SNS, EventBridge, Kafka with multiple consumer groups)
- Explicitly typed as `Event` or `INotification` (framework annotation)

### Command indicators

- Imperative name (`PlaceOrder`, `CancelPayment`, `SendEmail`)
- Sent to a specific queue / handler (single consumer)
- Producer expects the command to be processed (may or may not return a response)
- Framework annotation: `@CommandHandler`, `ICommand`, Axon `@CommandHandler`
- Celery task, Sidekiq job (usually commands)

### Query indicators

- Read-only naming (`GetOrder`, `FindUser`, `ListPayments`)
- Expects a synchronous response
- HTTP `GET`, gRPC unary RPC returning data
- Framework annotation: `@QueryHandler`, `IRequest<T>` with a return type

### When to mark uncertain

- Name doesn't clearly match any pattern (e.g., `OrderProcessed` — is that an event post-processing, or a command to process?)
- Evidence is only a type definition with no producer/consumer found
- Multiple indicators conflict (e.g., past-tense name but sent to a single queue expecting processing)

**Always grill uncertain messages in Phase 4 — do not silently pick.**
