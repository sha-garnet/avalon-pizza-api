# 🍕 AvalonPizza Server API

[![Build Status](https://img.shields.io/badge/.NET-10.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-CC2927.svg?logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server/)
[![Cache](https://img.shields.io/badge/Cache-Redis-DC382D.svg?logo=redis&logoColor=white)](https://redis.io/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20WSL2-0078D4.svg?logo=windows&logoColor=white)]()

A high-performance REST API built with .NET 10 and Dapper, optimized for sub-millisecond data retrieval. By utilizing a Database-First architecture with pre-compiled Stored Procedures and Redis distributed caching, the system minimizes database round-trips and eliminates ORM overhead. Features custom middleware for mapping SQL-state exceptions to granular HTTP responses, ensuring enterprise-grade reliability and observability.

## 📌 Project Overview
*   **Purpose:** A production-grade backend foundation for a scalable order management system, designed to demonstrate high-performance architectural patterns.
*   **Key Focus:** Showcasing the synergy between modern **.NET 10** features and optimized **SQL Server** stored procedures.
*   **Architecture:** Clean separation of concerns using the **Repository Pattern**, a dedicated **Service Layer**, and **Global Exception Middleware** for centralized error handling.
*   **Data Strategy:** A high-speed **Dapper-based** approach utilizing User-Defined Table Types (UDTTs) to minimize database round-trips.
*   **Primary Tech Stack:** C#, .NET 10, SQL Server, Redis, Docker Desktop (WSL 2).

## 🛠️ Key Architectural Features
*   **Distributed Caching:** Reduced DB load by caching static data (Toppings, Sizes, prices) in Redis.
*   **Defensive SQL Layer:** Business logic encapsulated in Stored Procedures using `TRY/CATCH` blocks and custom `THROW` states for granular error reporting.
*   **Atomic Transactions:** Ensures data consistency by utilizing SQL Transactions within stored procedures, guaranteeing that complex orders and topping mappings either succeed entirely or roll back safely.
*   **Idempotent Schema Management:** Custom `DbInitializer` ensures database creation and seed scripts can be executed repeatedly without side effects or data duplication.
*   **Health Monitoring:** Integrated Health Check API monitoring real-time connectivity for both SQL Server and Redis.
*   **Structured Logging:** Configured via **Serilog** to provide high-visibility console output and rolling file logs with a 7-day retention policy for efficient troubleshooting.


## 🚀 Getting Started

### Prerequisites
*   **OS:** Windows 11 + **WSL 2** (Ubuntu).
*   **Runtime:** [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
*   **Containerization:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) (WSL 2 Backend enabled).
*   **Database:** SQL Server (LocalDB or Express).

### Environment Setup
1.  **Spin up Redis:**

    ```bash
    docker run --name avalon-redis -p 6379:6379 -d redis
    ```
2.  **Configure `appsettings.Development.json`:**

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PizzaStoreDb;Trusted_Connection=True;TrustServerCertificate=True;",
        "RedisConnection": "localhost:6379"
      },
      "ApiKeySettings": {
        "ApiKey": "your-secret-key-here"
      }
    }
    ```
3.  **Log Directory:** Ensure the log directory exists on your machine (default: `C:\temp\log\`) or update the path in `appsettings.Development.json`.

    ```json
    {
      "LoggingPaths": {
        "PizzaLog": "C:\\temp\\log\\pizza_api_.txt"
      },
      "ConnectionStrings": { ... }
    }
    ```
### 📖 API Documentation

The API includes a **Custom Landing Page** at the root URL (`/`) which provides immediate status updates on system health. Full interactive documentation is available via **Swagger**.

### Primary Endpoints

| Method | Endpoint | SQL Procedure | Description |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/toppings` | `usp_Toppings_GetAll` | Retrieves all active toppings. Optimized with Redis caching. |
| **GET** | `/api/sizes` | `usp_PizzaSizes_GetAll` | Retrieves all pizza sizes and base prices. Optimized with Redis caching. |
| **GET** | `/api/orders/{id}` | `usp_Orders_GetById` | Fetches a specific order and its associated toppings via multiple result sets. |
| **POST** | `/api/orders` | `usp_Orders_Insert` | Places a new order. Maps toppings using the `ToppingListType` UDTT. |
| **PATCH** | `/api/orders/{id}/status` | `usp_Orders_UpdateStatus` | Updates the order lifecycle (e.g., Pending → Baking). Validates Status ID. |
| **PUT** | `/api/orders/{id}` | `usp_Orders_Update` | Updates order details and syncs toppings. Only allowed if status is `Pending`. |
| **DELETE** | `/api/orders/{id}` | `usp_Orders_Delete` | Performs a soft-delete (sets `IsActive = 0`). Only allowed if status is `Pending`. |

### 🚨 The "State" Dictionary (SQL Error Mapping)

This table maps the custom SQL `THROW` states to their corresponding business logic and the recommended HTTP response codes.

| SQL State | Logic Category | Description | Suggested HTTP Response |
| :--- | :--- | :--- | :--- |
| **1** | **Not Found** | The Order ID provided does not exist or is marked as inactive. | `404 Not Found` |
| **2** | **Business Rule** | Modification/Deletion attempt on an order that is no longer "Pending". | `422 Unprocessable Entity` |
| **3** | **Validation** | The provided `StatusId` does not exist in the `OrderStatus` lookup table. | `400 Bad Request` |
| **4** | **Validation** | Topping list is empty. Orders must have at least one topping. | `400 Bad Request` |
| **5** | **Validation** | One or more `ToppingId` values provided do not exist in the database. | `400 Bad Request` |