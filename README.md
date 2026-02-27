# AgroSolutions Sensor Ingestion API

API de ingestão de dados de sensores para o sistema AgroSolutions. Esta API recebe dados de sensores IoT e publica em uma fila RabbitMQ para processamento assíncrono.

## 🏗️ Arquitetura

O projeto segue uma arquitetura em camadas (Clean Architecture):

- **Core**: Interfaces e contratos
- **Application**: DTOs, Services e Exceptions
- **Infrastructure**: Implementações (RabbitMQ)
- **API**: Endpoints, Middlewares e Configuração

## 🚀 Tecnologias

- .NET 8.0
- RabbitMQ Client 6.8.1
- JWT Authentication
- Minimal API
- Swagger/OpenAPI

## 📋 Funcionalidades

- ✅ Recepção de dados de sensores via HTTP POST
- ✅ Autenticação JWT obrigatória
- ✅ Validação de dados com DataAnnotations
- ✅ Publicação em fila RabbitMQ
- ✅ Middleware de correlação de requisições
- ✅ Middleware de tratamento de exceções
- ✅ Documentação Swagger

## 🔧 Configuração

### Variáveis de Ambiente

As seguintes variáveis de ambiente devem ser configuradas:

#### JWT (obrigatório)
```bash
Jwt__Key=sua-chave-secreta-super-segura-com-no-minimo-32-caracteres
Jwt__Issuer=AgroSolutions
Jwt__Audience=AgroSolutions.API
```

#### RabbitMQ (obrigatório)
```bash
RabbitMQ__Host=localhost
RabbitMQ__Port=5672
RabbitMQ__Username=guest
RabbitMQ__Password=guest
RabbitMQ__QueueName=sensor-data-queue
```

### appsettings.json

O arquivo `appsettings.json` já está configurado com valores padrão vazios. As variáveis de ambiente têm precedência.

## 📡 Endpoint

### POST /api/sensor-data

Recebe dados de um sensor e publica na fila RabbitMQ.

**Autenticação:** Bearer Token (JWT) obrigatório

**Request Body:**
```json
{
  "fieldId": 1,
  "sensorType": "Temperature",
  "value": 25.5,
  "timestamp": "2026-02-07T10:30:00Z"
}
```

**Validações:**
- `fieldId`: Obrigatório, maior que 0
- `sensorType`: Obrigatório, máximo 100 caracteres
- `value`: Obrigatório
- `timestamp`: Obrigatório, não pode estar no futuro (mais de 5 minutos)

**Resposta de Sucesso (202 Accepted):**
```json
{
  "message": "Dados do sensor recebidos e publicados com sucesso",
  "timestamp": "2026-02-07T10:30:01Z"
}
```

**Resposta de Erro (400 Bad Request):**
```json
{
  "statusCode": 400,
  "message": "O ID do talhão deve ser maior que zero",
  "timestamp": "2026-02-07T10:30:01Z"
}
```

## 🐰 RabbitMQ

A API publica mensagens na fila configurada com as seguintes características:

- **Exchange**: (default)
- **Queue**: Configurável via `RabbitMQ__QueueName`
- **Durable**: true
- **Persistent**: true
- **Content-Type**: application/json
- **Formato JSON**: camelCase

Exemplo de mensagem publicada:
```json
{
  "fieldId": 1,
  "sensorType": "Temperature",
  "value": 25.5,
  "timestamp": "2026-02-07T10:30:00Z"
}
```

## 🏃 Como Executar

### Pré-requisitos
- .NET 8.0 SDK
- RabbitMQ rodando (Docker ou instalação local)

### Docker RabbitMQ
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### Restaurar Dependências
```bash
dotnet restore
```

### Executar a API
```bash
cd AgroSolutions.Sensor.Ingestion
dotnet run
```

A API estará disponível em:
- HTTPS: https://localhost:7XXX
- HTTP: http://localhost:5XXX
- Swagger: https://localhost:7XXX/swagger

## 🧪 Testando a API

### 1. Obter Token JWT

Primeiro, você precisa de um token JWT válido do serviço de autenticação (`agro-solutions-users`).

### 2. Fazer Requisição

```bash
curl -X POST https://localhost:7XXX/api/sensor-data \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "fieldId": 1,
    "sensorType": "Temperature",
    "value": 25.5,
    "timestamp": "2026-02-07T10:30:00Z"
  }'
```

### 3. Verificar no RabbitMQ

Acesse o painel de gerenciamento do RabbitMQ:
- URL: http://localhost:15672
- User: guest
- Password: guest

Navegue até a fila `sensor-data-queue` para ver as mensagens publicadas.

## 📊 Middlewares

### CorrelationMiddleware
Adiciona um ID de correlação (X-Correlation-ID) a cada requisição para rastreamento.

### ExceptionHandlingMiddleware
Trata exceções e retorna respostas HTTP apropriadas:
- 400: ValidationException
- 422: BusinessException
- 500: Outros erros

## 🔐 Segurança

- ✅ Autenticação JWT obrigatória
- ✅ Validação de token (Issuer, Audience, Lifetime, Signature)
- ✅ HTTPS redirection
- ✅ Validação de entrada de dados

## 📝 Logs

A API gera logs estruturados com informações de:
- Início e fim de requisições
- Dados do sensor recebidos
- Publicação na fila RabbitMQ
- Erros e exceções
- Correlation ID para rastreamento

## 🏗️ Estrutura de Pastas

```
agro-solutions-sensor-ingestion/
├── AgroSolutions.Sensor.Ingestion/    # API Layer
│   ├── Middlewares/
│   │   ├── CorrelationMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
├── Application/                        # Application Layer
│   ├── DTOs/
│   │   └── SensorDataDto.cs
│   ├── Exceptions/
│   │   ├── BusinessException.cs
│   │   └── ValidationException.cs
│   └── Services/
│       └── SensorIngestionAppService.cs
├── Core/                               # Domain Layer
│   └── Interfaces/
│       └── IRabbitMqPublisher.cs
└── Infrastructure/                     # Infrastructure Layer
    ├── Messaging/
    │   ├── RabbitMqPublisher.cs
    │   └── RabbitMqSettings.cs
    └── DependencyInjection.cs
```

## 🔄 Integração com Outros Serviços

Esta API faz parte do ecossistema AgroSolutions e integra-se com:
- **agro-solutions-users**: Para autenticação JWT
- **agro-solutions-properties-fields**: IDs dos talhões devem existir neste serviço
- **Consumidores RabbitMQ**: Serviços que processam os dados dos sensores

## 📈 Próximos Passos

- [ ] Implementar Health Checks
- [ ] Adicionar métricas e monitoramento
- [ ] Implementar rate limiting
- [ ] Adicionar testes unitários e de integração
- [ ] Implementar dead letter queue para mensagens com falha
- [ ] Adicionar suporte a batch de sensores
