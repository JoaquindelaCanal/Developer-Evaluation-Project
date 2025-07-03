# Ambev Developer Evaluation - Sales Management API

This repository contains a .NET 8 Web API developed as part of the Ambev Developer Evaluation. It implements a core set of features for managing sales, following a Clean Architecture approach with a focus on maintainability, testability, and scalability.

## Table of Contents
1.  [Project Overview](#1-project-overview)
2.  [Architecture](#2-architecture)
3.  [Key Technologies & Libraries](#3-key-technologies--libraries)
4.  [Setup and Configuration](#4-setup-and-configuration)
    * [Prerequisites](#prerequisites)
    * [Database Setup](#database-setup)
    * [Project Structure](#project-structure)
5.  [How to Run the Project](#5-how-to-run-the-project)
    * [Via Visual Studio / Rider](#via-visual-studio--rider)
    * [Via .NET CLI](#via-net-cli)
    * [Accessing the API (Swagger UI)](#accessing-the-api-swagger-ui)
6.  [API Endpoints](#6-api-endpoints)
    * [Sales Endpoints](#sales-endpoints)
7.  [Testing](#7-testing)
    * [Running Tests](#running-tests)
    * [Manual Testing (via Swagger/Postman)](#manual-testing-via-swaggerpostman)
8.  [Documentation & Design Considerations](#8-documentation--design-considerations)
    * [Clean Architecture Principles](#clean-architecture-principles)
    * [MediatR (CQRS & Domain Events)](#mediatr-cqrs--domain-events)
    * [AutoMapper](#automapper)
    * [Dynamic Querying (Filtering, Sorting, Pagination)](#dynamic-querying-filtering-sorting-pagination)
    * [Rebus (Event Publishing)](#rebus-event-publishing)
    * [Custom Exception Handling](#custom-exception-handling)
    * [Dependency Injection](#dependency-injection)
    * [Overall Organization](#overall-organization)
9.  [Future Enhancements](#9-future-enhancements)


---

## 1. Project Overview

This project provides a RESTful API for managing sales. It demonstrates the implementation of:
* CRUD (Create, Read, Update, Delete) operations for `Sale` entities.
* Advanced listing capabilities including pagination, dynamic filtering, and dynamic sorting.
* Integration event publishing using Rebus to signal changes in the Sales domain (e.g., `SaleCreated`, `SaleModified`, `SaleCancelled`, `SaleItemCancelled`).

## 2. Architecture

The project adheres to **Clean Architecture** principles, separating concerns into distinct layers:

* **`AmbevDeveloperEvaluation.WebApi`**: The entry point for HTTP requests. Contains controllers, API-specific models (like `PaginatedResponse`), and application startup configuration. It depends only on the Application layer.
* **`AmbevDeveloperEvaluation.Application`**: Contains the application's business logic, use cases (Commands and Queries), DTOs, interfaces for external services (e.g., `ISaleRepository`), and MediatR handlers. It orchestrates the flow of data between the UI and the domain/infrastructure. It depends on `Domain` and `Infrastructure` (for common models like `PaginatedList`).
* **`AmbevDeveloperEvaluation.Domain`**: Encapsulates the core business rules and entities (`Sale`, `SaleItem`). It is independent of all other layers. It also contains Domain Events.
* **`AmbevDeveloperEvaluation.Infrastructure`**: Implements the interfaces defined in the Application layer. This includes data persistence (EF Core, repositories), external service integrations, and common infrastructure utilities (like `PaginatedList` utility for database querying). It depends on `Domain`.
* **`AmbevDeveloperEvaluation.Common`**: A shared library for cross-cutting concerns like custom exceptions, common models (e.g., query parameters), and most importantly, **Integration Event Contracts** (the messages published via Rebus). This project is referenced by all other relevant layers.

**Dependency Flow:** `WebApi` -> `Application` -> (`Domain`, `Infrastructure`, `Common`)

## 3. Key Technologies & Libraries

* **.NET 8**: The core framework for the application.
* **ASP.NET Core**: For building the Web API.
* **Entity Framework Core**: ORM for database interactions.
    * **SQL Server**: Default database provider.
* **MediatR**: For implementing CQRS (Command Query Responsibility Segregation) and Domain Events.
* **AutoMapper**: For object-to-object mapping (e.g., between entities and DTOs).
* **System.Linq.Dynamic.Core**: Enables dynamic filtering and sorting capabilities based on string expressions.
* **Rebus**: A lightweight service bus for robust messaging and event publishing.
    * **Rebus.Transport.InMemory**: For in-memory message transport (used for local development).
* **Swagger/OpenAPI**: For API documentation and testing via Swagger UI.
* **Serilog**: (Optional, but configured in `Program.cs`) For structured logging.


## 4. Setup and Configuration

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Git](https://git-scm.com/downloads)
* (Optional) SQL Server installed or access to a SQL Server instance. If not, EF Core will create a localdb instance for you automatically.
* (Optional) An IDE like Visual Studio 2022 or Rider.

### Database Setup

1.  **Connection String:**
    The database connection string is configured in `AmbevDeveloperEvaluation.WebApi/appsettings.json`. By default, it's set up for a LocalDB instance, which will be automatically created by EF Core.

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AmbevSalesDb;Trusted_Connection=True;MultipleActiveResultSets=true"
      }
    }
    ```
    If you wish to use a different SQL Server instance, update this connection string accordingly.

2.  **Apply Migrations:**
    Navigate to the `AmbevDeveloperEvaluation.Infrastructure` project directory in your terminal:

    ```bash
    cd src/AmbevDeveloperEvaluation.Infrastructure
    ```

    Apply the database migrations to create the database schema:

    ```bash
    dotnet ef database update
    ```
    This command will create the `AmbevSalesDb` database (or connect to an existing one) and apply all pending migrations, setting up the necessary tables for `Sale` and `SaleItem` entities.

### Project Structure

The solution follows a well-defined Clean Architecture structure:

├── src/
│   ├── AmbevDeveloperEvaluation.Application/
│   │   ├── Common/        # Common interfaces, models, exceptions, behaviors
│   │   ├── DTOs/          # Data Transfer Objects
│   │   ├── Features/      # Grouped by feature (e.g., Sales)
│   │   │   ├── Sales/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateSale/
│   │   │   │   │   ├── DeleteSale/
│   │   │   │   │   └── UpdateSale/
│   │   │   │   ├── Events/   # MediatR Notification handlers for Rebus event publishing
│   │   │   │   └── Queries/
│   │   │   │       ├── GetSaleById/
│   │   │   │       └── ListSales/
│   │   │   └── ...
│   │   ├── Interfaces/    # Repository interfaces
│   │   └── Mappings/      # AutoMapper profiles
│   ├── AmbevDeveloperEvaluation.Common/
│   │   ├── Contracts/     # Rebus Integration Event contracts
│   │   └── ...
│   ├── AmbevDeveloperEvaluation.Domain/
│   │   ├── Entities/      # Core business entities
│   │   ├── Enums/         # Domain-specific enums
│   │   └── Events/        # MediatR Domain Events
│   ├── AmbevDeveloperEvaluation.Infrastructure/
│   │   ├── Common/        # Common infrastructure utilities (e.g., PaginatedList)
│   │   ├── ORM/           # EF Core DbContext and configurations
│   │   │   └── Repositories/ # Concrete repository implementations
│   │   └── ...
│   └── AmbevDeveloperEvaluation.WebApi/
│       ├── Common/        # API-specific utilities, middleware, response models
│       ├── Controllers/   # API controllers
│       ├── appsettings.json # Configuration files
│       └── Program.cs     # Application startup
│
└── .gitignore
└── AmbevDeveloperEvaluation.sln
└── README.md



## 5. How to Run the Project

### Via Visual Studio / Rider

1.  Open the `AmbevDeveloperEvaluation.sln` file in Visual Studio or Rider.
2.  Set `AmbevDeveloperEvaluation.WebApi` as the startup project.
3.  Press `F5` or click the "Run" button to build and run the application.

### Via .NET CLI

1.  Navigate to the `AmbevDeveloperEvaluation.WebApi` project directory in your terminal:
    ```bash
    cd src/AmbevDeveloperEvaluation.WebApi
    ```
2.  Run the application:
    ```bash
    dotnet run
    ```
    This will start the Kestrel web server, typically on `https://localhost:7001` (or a similar port).

### Accessing the API (Swagger UI)

Once the application is running, open your web browser and navigate to the Swagger UI:

`https://localhost:7001/swagger/index.html` (or the corresponding port shown in your terminal/IDE).

Here, you can explore the available endpoints, view their documentation, and execute requests directly.

## 6. API Endpoints

The API base URL is typically `https://localhost:7001/api`.

### Sales Endpoints

* **`GET /api/Sales`**
    * **Description**: Retrieves a paginated list of sales with dynamic filtering and sorting.
    * **Query Parameters**:
        * `page` (int, default: 1): Page number.
        * `size` (int, default: 10): Number of items per page.
        * `sort` (string[], optional): Sorting criteria. Format: `"FieldName [asc|desc]"`.
            * Example: `?sort=SaleDate desc&sort=ClientName asc`
        * `filter` (Dictionary<string, string[]>, optional): Filtering criteria. Format: `"FieldName=Value"` or `"_minFieldName=Value"`, `"_maxFieldName=Value"`. Supports `Equals`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`.
            * Example: `?filter[ClientName]=John*&filter[Status]=Pending&filter[_minTotalAmount]=100`
    * **Responses**: `200 OK` (PaginatedResponse<SaleDto>), `400 Bad Request`

* **`POST /api/Sales`**
    * **Description**: Creates a new sale.
    * **Request Body**: `CreateSaleCommand` (JSON)
        ```json
        {
          "clientName": "Ambev Customer 1",
          "saleDate": "2025-07-03T10:00:00Z",
          "items": [
            {
              "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
              "quantity": 2,
              "unitPrice": 10.50
            },
            {
              "productId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
              "quantity": 1,
              "unitPrice": 25.00
            }
          ]
        }
        ```
    * **Responses**: `201 Created` (ApiResponseWithData<Guid>), `400 Bad Request`, `500 Internal Server Error`

* **`GET /api/Sales/{id}`**
    * **Description**: Retrieves a sale by its unique identifier.
    * **Path Parameter**: `id` (GUID)
    * **Responses**: `200 OK` (ApiResponseWithData<SaleDto>), `404 Not Found`, `500 Internal Server Error`

* **`PUT /api/Sales/{id}`**
    * **Description**: Updates an existing sale.
    * **Path Parameter**: `id` (GUID)
    * **Request Body**: `UpdateSaleCommand` (JSON) - similar to `CreateSaleCommand` but with fields to update.
        ```json
        {
          "id": "existing-sale-guid",
          "clientName": "Updated Client Name",
          "status": "Completed",
          "items": [
            // Include items to update, add, or potentially mark as cancelled if your command supports it
          ]
        }
        ```
    * **Responses**: `204 No Content`, `400 Bad Request`, `404 Not Found`, `500 Internal Server Error`

* **`DELETE /api/Sales/{id}`**
    * **Description**: Deletes a sale by its unique identifier.
    * **Path Parameter**: `id` (GUID)
    * **Responses**: `204 No Content`, `404 Not Found`, `500 Internal Server Error`

## 7. Testing

### Running Tests

If unit or integration test projects were included, you would run them as follows:

1.  Navigate to your solution's root directory:
    ```bash
    cd Teste\ AMBEV/template/backend # Or simply the solution root
    ```
2.  Execute all tests:
    ```bash
    dotnet test
    ```
    (Note: Specific test projects were not developed as part of this interactive session, but the architecture facilitates easy testing).

### Manual Testing (via Swagger/Postman)

The easiest way to test the API endpoints is by using the **Swagger UI** (`https://localhost:7001/swagger/index.html`).

Alternatively, you can use tools like **Postman** or **cURL** to send requests:

**Example: Create a Sale (POST)**

bash
curl -X POST "https://localhost:7001/api/Sales" \
-H "accept: */*" \
-H "Content-Type: application/json" \
-d '{
  "clientName": "Test Client",
  "saleDate": "2025-07-03T14:00:00Z",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 1,
      "unitPrice": 50.00
    }
  ]
}'



Example: Get all Sales (GET)

Bash

curl -X GET "https://localhost:7001/api/Sales?page=1&size=2&sort=SaleDate desc" \
-H "accept: application/json"


8. Documentation & Design Considerations
Clean Architecture Principles
The project strictly adheres to Clean Architecture principles:

Separation of Concerns: Each layer has a distinct responsibility.

Dependency Rule: Inner layers have no knowledge of outer layers. This is enforced by defining interfaces in inner layers (e.g., Application) and implementing them in outer layers (e.g., Infrastructure).

Independent of Frameworks: Business logic is not tied to any specific framework (ASP.NET Core, EF Core, etc.), making it highly testable and adaptable.

MediatR (CQRS & Domain Events)
CQRS: Commands (for writes like CreateSaleCommand) and Queries (for reads like ListSalesQuery) are separated, handled by dedicated handlers. This enhances clarity, allows for independent scaling, and specific optimizations for read/write models.

Domain Events: Internal domain events (e.g., SaleCreatedEvent) are published via MediatR's IPublisher. These events signal that something important happened in the domain.

AutoMapper
Used extensively for mapping between:

Command/Query DTOs and Domain Entities.

Domain Entities and Response DTOs.

ApplicationPaginatedList and PaginatedResponse (via PaginationProfile).

Dynamic Querying (Filtering, Sorting, Pagination)
The ListSalesQueryHandler leverages System.Linq.Dynamic.Core to provide highly flexible API endpoints for searching and Browse sales data without needing to hardcode every possible filter or sort combination. This significantly reduces boilerplate code and increases API flexibility for consumers.

Rebus (Event Publishing)
Integration Events: The project demonstrates a robust event-driven architecture by publishing integration events (e.g., SaleCreatedIntegrationEvent) using Rebus. These events act as a contract for other services or systems to react to changes in the Sales domain.

Bridge between Domain & Integration Events: MediatR INotificationHandlers are used as a bridge. A domain event (e.g., SaleCreatedEvent) triggers a specific INotificationHandler, which then maps the data to a Rebus message and publishes it onto the message bus.

In-Memory Transport: For simplicity and testing purposes, Rebus is configured to use an in-memory transport (Rebus.Transport.InMemory). In a production environment, this would typically be replaced with a persistent message queue like RabbitMQ, Azure Service Bus, or Amazon SQS.

Custom Exception Handling
A custom ExceptionHandlingMiddleware is implemented in the WebApi layer to catch application-specific exceptions (like BadRequestException, NotFoundException) and return consistent, well-structured API error responses (ApiResponse). This avoids repetitive try-catch blocks in controllers and improves the user experience for API consumers.

Dependency Injection
The project extensively uses .NET Core's built-in Dependency Injection container. Services, repositories, MediatR handlers, AutoMapper profiles, and the Rebus bus are all registered and injected into their respective consumers, promoting loose coupling and testability.

Overall Organization
The project's structure is modular and intuitive, making it easy to locate specific functionalities. Features are grouped logically, and common utilities are placed in shared libraries, ensuring reusability and maintainability. The use of interfaces throughout the application fosters loose coupling and facilitates mocking for testing.

9. Future Enhancements
Robust Validation: Implement FluentValidation for more comprehensive and declarative validation rules for commands and queries.

Unit and Integration Tests: Add dedicated test projects to cover core business logic and API interactions.

Authentication & Authorization: Integrate JWT Bearer token authentication and role-based authorization.

Logging: Deeper integration of structured logging (e.g., Serilog) for better observability.

Idempotency: For POST/PUT operations, consider implementing idempotency keys to handle duplicate requests safely.

Database Seeding: Add initial data seeding for easier development and testing.

Production Rebus Transport: Replace Rebus.Transport.InMemory with a production-ready message queue (e.g., Rebus.RabbitMq) and implement consumer services.

API Versioning: Implement API versioning (e.g., v1, v2) to manage changes gracefully.
