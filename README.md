# Catalyst

Catalyst is a modern, modular backend framework for building robust APIs and services with .NET. It’s designed with clean architecture principles, supporting scalable, high-performance solutions for any business domain.

## Features

- **Modular Architecture:** Clearly separated layers for core logic, data access, and infrastructure.
- **Repository & Unit of Work Patterns:** Abstract data access for maintainability and testability.
- **MediatR Integration:** Decoupled request/response and pipeline behaviors (including Unit of Work).
- **Observability:** Built-in logging, health checks, and Prometheus metrics for monitoring.
- **Strong Service Registration:** Extension methods enable clean, organized service setup.
- **Extensible Modules:** Easily add new modules and features without breaking existing code.

## Technologies

- .NET 9
- Entity Framework Core
- MediatR
- Serilog for logging
- Prometheus-net for metrics
- ASP.NET Core Web API

## Getting Started

1. Clone the repository.
2. Update your connection strings and configuration as needed.
3. Run database migrations if required.
4. Launch the API project.

## Project Structure

- `Catalyst.Api`: Entry point for the API, configures services and middleware.
- `Catalyst.Core`: Core abstractions, base controllers, MediatR behaviors.
- `Catalyst.Data`: Entity Framework repositories and data access logic.
- `Catalyst.ExampleModule`: Example extension module showing recommended patterns.

## Contributing

Pull requests, issues, and suggestions are welcome. Please keep contributions modular and aligned with clean code principles.

---

*Catalyst is built for high code quality and maintainability—help us make it even better!*
