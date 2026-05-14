# AvalonPizza Server API

[![Build Status](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-red.svg)](https://www.microsoft.com/en-us/sql-server/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20WSL2-lightgrey.svg)]()

A robust backend REST API for managing pizza orders, featuring Redis caching and a "Database-First" stored procedure architecture.

## 📌 Project Overview
*   **Purpose:** [Fill in: e.g., A backend system designed to handle real-time pizza customization and order lifecycle management.]
*   **Primary Tech Stack:** C#, .NET 8, SQL Server, Redis, Docker Desktop.

## 🚀 Getting Started

### Prerequisites
*   **Operating System:** Windows 10/11 with **WSL 2** (Ubuntu distribution recommended).
*   **Runtime:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
*   **Containerization:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Integration with WSL 2 enabled).
*   **Database Tool:** Azure Data Studio or SQL Server Management Studio (SSMS).

### Database Setup
1.  Connect to your local SQL Server instance.
2.  Execute the master script located at `/Database/TablesAndProcs.sql`.
    *   This script initializes `PizzaStoreDb`.
    *   Creates relational tables and User-Defined Table Types (`ToppingListType`).
    *   Seeds initial data for `Toppings`, `PizzaSizes`, and `OrderStatus`.
    *   Deploys all defensive Stored Procedures.

### Local Environment Configuration
Create/Update `appsettings.Development.json` in the `AvalonPizza.Server` project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PizzaStoreDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}

### 📖 API Documentation (Endpoints)

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

This table maps the custom SQL `THROW` states to their corresponding business logic and the recommended HTTP response codes for the C# Middleware.

| SQL State | Logic Category | Description | Suggested HTTP Response |
| :--- | :--- | :--- | :--- |
| **1** | **Not Found** | The Order ID provided does not exist or is marked as inactive. | `404 Not Found` |
| **2** | **Business Rule** | Modification/Deletion attempt on an order that is no longer "Pending". | `422 Unprocessable Entity` |
| **3** | **Validation** | The provided `StatusId` does not exist in the `OrderStatus` lookup table. | `400 Bad Request` |
| **4** | **Validation** | Topping list is empty. Orders must have at least one topping. | `400 Bad Request` |
| **5** | **Validation** | One or more `ToppingId` values provided do not exist in the database. | `400 Bad Request` |