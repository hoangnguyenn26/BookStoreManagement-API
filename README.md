<div align="center">

# **BookStoreManagement-API**
### **ASP.NET Core Clean Architecture Bookstore Management API**

A powerful and scalable backend API for modern bookstore management systems.  
*(Empowering Bookstores – Simplifying Operations)*

[![.NET Version](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![Last Commit](https://img.shields.io/github/last-commit/hoangnguyenn26/BookStoreManagement-API)](https://github.com/hoangnguyenn26/BookStoreManagement-API/commits/main)
[![Languages](https://img.shields.io/github/languages/count/hoangnguyenn26/BookStoreManagement-API)](https://github.com/hoangnguyenn26/BookStoreManagement-API)
[![License](https://img.shields.io/github/license/hoangnguyenn26/BookStoreManagement-API)](https://github.com/hoangnguyenn26/BookStoreManagement-API/blob/main/LICENSE)

---

### 🔍 **Keywords**
`Bookstore Management API`, `ASP.NET Core Web API`, `Bookstore Backend`,  
`Book Management System`, `Clean Architecture API`, `EF Core Bookstore`,  
`Inventory Management API`, `Order Management API`

</div>

---

## 📘 **Overview**

**BookStoreManagement-API** is a robust **ASP.NET Core Bookstore Management API** built with **Clean Architecture**, designed to support modern bookstore operations.  
The project focuses on:

- High performance  
- Security  
- Extensibility  
- Ease of maintenance  

It serves as an excellent foundation for building bookstore systems, retail management platforms, or scalable e-commerce backends.

---

## 🏛️ **Architecture (Clean / Layered Architecture)**

The project follows a clean separation of concerns across four layers:

### **1. Domain Layer – Core Business Model**
- Entities and Value Objects  
- Domain Services  
- Business rules and domain logic  
- Domain Interfaces (Repositories, Services)

### **2. Application Layer – Use Cases**
- DTOs and Validators (FluentValidation)  
- Application Services  
- AutoMapper Profiles  
- Business workflows  

### **3. Infrastructure Layer – Technical Implementations**
- EF Core DbContext (Code-First)  
- Repository + Unit of Work patterns  
- Email / Token services  
- Logging, caching, integrations

### **4. API Layer – Presentation**
- Controllers  
- Custom Middleware (Exception Handling, Logging)  
- Dependency Injection setup  
- Authentication & Authorization  
- Swagger / OpenAPI documentation  
- API Versioning  

---

## 🚀 **Key Features**

### 👤 **User Management**
- Registration & Login  
- JWT Authentication  
- Role-based Authorization (Admin, Staff, User)  
- User profiles & addresses  

### 📚 **Book & Catalog Management**
- CRUD for Books, Authors, and Categories  
- Nested category hierarchy  
- Automatic stock updates  

### 🛒 **Cart & Order Management**
- Server-side shopping cart  
- Checkout workflow  
- Online & In-store order creation  
- Order state tracking  
- Snapshot pricing/address per order  

### 🏷️ **Coupon & Promotion System**
- Discount code generation  
- Usage tracking  
- Configurable conditions  

### ⭐ **Book Reviews**
- User reviews  
- Admin moderation  

### 📊 **Dashboard & Reports**
- Revenue analytics  
- Top-selling books  
- Low-stock tracking  
- User activity insights  

### ⚠️ **Centralized Error Handling**
- Unified error responses  
- Detailed request logging  
- Handled via custom middleware  

### 🔐 **Security**
- JWT Authentication  
- BCrypt password hashing  
- Role-based access control  

### 🔬 **Testing**
- Unit tests (xUnit + Moq)  
- Integration tests using `Microsoft.AspNetCore.Mvc.Testing`  
- Performance tests via **K6**

---

## 🛠️ **Tech Stack**

| Technology | Purpose |
|------------|---------|
| **ASP.NET Core 8** | Main backend framework |
| **C# 12** | Programming language |
| **Entity Framework Core 8** | ORM (Code-First) |
| **SQL Server** | Primary database |
| **AutoMapper** | Object mapping |
| **FluentValidation** | Request validation |
| **BCrypt.Net** | Password hashing |
| **JWT Bearer** | Authentication |
| **Serilog** | Logging |
| **Swagger / OpenAPI** | API documentation |
| **xUnit + Moq** | Testing |
| **API Versioning** | Manage API versions |

---

## 🧪 **Testing**

Run all tests:

```bash
dotnet test
```
### 🔥 **K6-performance Testing**
<img width="974" height="639" alt="image" src="https://github.com/user-attachments/assets/39755fb4-6e7c-49aa-8d1c-8b0466f27963" />
