# 🍕 AvalonPizza Server API

[![Build Status](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Cloud](https://img.shields.io/badge/Cloud-AWS-232F3E.svg?logo=amazon-aws&logoColor=white)](https://aws.amazon.com/)
[![IaC](https://img.shields.io/badge/IaC-CloudFormation-orange.svg?logo=amazon-aws&logoColor=white)](https://aws.amazon.com/cloudformation/)
[![Config](https://img.shields.io/badge/SSM-Parameter_Store-232F3E.svg?logo=aws-systems-manager&logoColor=white)](https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-parameter-store.html)
[![Database](https://img.shields.io/badge/RDS-SQL_Server-CC2927.svg?logo=amazon-rds&logoColor=white)](https://aws.amazon.com/rds/sqlserver/)
[![ORM](https://img.shields.io/badge/ORM-Dapper-007ACC.svg?logo=dotnet&logoColor=white)](https://dapperlib.github.io/Dapper/)
[![Migration](https://img.shields.io/badge/Migration-DbUp-007ACC.svg?logo=nuget&logoColor=white)](https://dbup.readthedocs.io/)
[![Cache](https://img.shields.io/badge/Cache-Redis-DC382D.svg?logo=redis&logoColor=white)](https://redis.io/)
[![Testing](https://img.shields.io/badge/Testing-xUnit-green.svg?logo=xunit&logoColor=white)]()
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20WSL2-0078D4.svg?logo=windows&logoColor=white)]()

A cloud-native REST API built with **.NET 8** and **Dapper**. By utilizing a Database-First architecture with pre-compiled Stored Procedures and Redis distributed caching, the system minimizes database round-trips and eliminates ORM overhead. This project demonstrates a serverless architecture, featuring automated infrastructure provisioning, decoupled schema migrations, and enterprise-grade observability.

---

## 📌 Project Overview
* **Purpose:** The foundations of a production-grade, serverless backend designed for a scalable order management system.
* **Cloud-Native Focus:** Built to run natively on **AWS Serverless** infrastructure, leveraging **AWS Lambda** for compute and **RDS (SQL Server)** for managed relational data storage.
* **Architecture:** Implements a decoupled, event-driven philosophy using a dedicated **Service Layer**, **Repository Pattern**, and **Global Exception Middleware** to ensure enterprise-grade reliability.
* **Data Strategy:** Optimized for performance through **Dapper-based** micro-ORM patterns and **User-Defined Table Types (UDTTs)**, minimizing database round-trips and infrastructure latency.
* **Automated Reliability:** Prioritizes "Infrastructure-as-Code" (IaC) and automated schema migration workflows to ensure consistent, repeatable deployments across development and production environments.

## 🛠️ Key Architectural Features
* **Infrastructure-as-Code:** 100% automated provisioning via **AWS CloudFormation**.
* **Serverless Compute:** Optimized for **AWS Lambda**, providing a scalable, cost-effective hosting model.
* **Decoupled Migration Engine:** Schema management and stored procedure versioning are extracted into a standalone **DbUp** project, enabling repeatable deployments.
* **Security:** Credentials managed via **AWS SSM Parameter Store**; database instances isolated in a potential private subnets with VPC Endpoints.
* **Defensive SQL Layer:** Business logic encapsulated in Stored Procedures with custom `THROW` states for granular error reporting.
* **Observability:** Structured logging via **Serilog** and integrated Health Check endpoints for real-time monitoring.

---

## 🚀 Deployment Strategy
The system follows a two-tier CloudFormation strategy to ensure separation of concerns. Deployment is fully automated via PowerShell scripts:

| Script | Purpose | Template Path |
| :--- | :--- | :--- |
| `deploy-infra.ps1` | Provisions VPC, Private Subnets, RDS, and SSM endpoints. | `base-stack.template` |
| `deploy-api.ps1` | Deploys Lambda functions and API Gateway. | `serverless.template` |

### Automated Migrations
The `AvalonPizza.Migrator` project ensures the database schema remains synchronized across environments:
* **Development:** Targets `(localdb)\MSSQLLocalDB`.
* **Production:** Targets AWS RDS, retrieving credentials securely from SSM.

---

## 📖 API Documentation & Sandbox
The API uses **OpenAPI/Swagger (via Swashbuckle)** to provide an interactive developer experience.

* 🏠 **Landing Page:** Health status and API documentation entry point at `/`.
* 🌐 **Sandbox:** Swagger UI available at `/swagger/index.html` (Development only).

### Primary Endpoints
| Method | Endpoint | SQL Procedure | Description |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/toppings` | `usp_Toppings_GetAll` | Retrieves all active toppings and prices. Optimized with Redis caching. |
| **GET** | `/api/pizzas/sizes` | `usp_PizzaSizes_GetAll` | Retrieves all pizza sizes and base prices. Optimized with Redis caching. |
| **GET** | `/api/orders/{id}` | `usp_Orders_GetById` | Fetches a specific order and its associated toppings via multiple result sets. |
| **POST** | `/api/orders` | `usp_Orders_Insert` | Places a new order. |
| **PUT** | `/api/orders/{id}` | `usp_Orders_Update` | Updates the order size and toppings. Only allowed if status is `Pending`. |
| **PATCH** | `/api/orders/{id}/status` | `usp_Orders_UpdateStatus` | (ApiKeyRequired) Updates the order lifecycle (e.g., Pending → Baking). |
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

---

## 🧪 Testing Suite
Built on **xUnit** and **Moq**, the test suite focuses on validating API boundaries without requiring an active database.

* 🛠️ **AAA Pattern:** Strict Arrange-Act-Assert structure for readability.
* 🎭 **Mocking:** Uses `Mock<T>` to isolate controllers from operational dependencies (`ILogger`, `IMapper`).