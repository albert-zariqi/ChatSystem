# Chat System
A distributed system designed to manage and monitor chat sessions efficiently, with a focus on scalability and fault tolerance.

## Key Features
- **ASP.NET Core**
- **Mediator and CQRS pattern**
- **Domain-Driven Design**
- **Redis for Caching**
- **RabbitMQ as FIFO Queue**
- **Global Exception Handling**
- **Structured Logging**
- **Fully Compliant with Dependency Injection**
- **Dockerized**


## Technology Stack
- **ASP.NET Core Web API**
- **Redis for Caching**
- **RabbitMQ for Queue-based Communication**
- **CQRS and MediatR for Request Handling**
- **Domain-Driven Design (DDD)**

## How To Use

### Clone the Repository
```bash
# Clone this repository
$ git clone https://github.com/albert-zariqi/ChatSystem
```

## Running Locally

- Open the solution in Visual Studio 2022.
- Ensure that RabbitMQ and Redis are running locally.
- Start ChatSystem.Coordinator.App, ChatSystem.Chat.API, ChatSystem.Presentation and ChatSystem.ApiGateway at the same time.
- Make sure the database connection is properly configured.

## Running in Docker

- Navigate to the directory where the solution is located.
- Ensure Docker is installed and running.
- Open a terminal and run:

```bash
Copy code
docker-compose up
```

## Credits
This software utilizes the following open-source technologies:

- ASP.NET Core
- MediatR
- Redis
- RabbitMQ
- Docker
- Serilog
