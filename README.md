# TodoAPI

A simple and clean Todo List RESTful Web API built with ASP.NET Core (.NET 8).  
This API allows you to create, read, update, and delete (CRUD) todo items.

## 🔧 Technologies Used

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- AutoMapper
- SQL Server
- Swagger (OpenAPI)
- Postman (for testing)

## 📁 Project Structure

- `Controllers/` - API endpoint logic (TodoController.cs)
- `Services/` - Business logic layer (TodoServices.cs)
- `Data/` - EF Core DbContext and database configuration
- `Contracts/` - Request/response DTOs
- `Models/` - Entity models

## 🚀 How to Run the Project

- 1.Clone the repository:
  git clone https://github.com/hasanackl/TodoAPI.git
- 2.Navigate into the project folder:
  cd TodoAPI
- 3.Restore packages:
  dotnet restore
- 4.Update the database:
  dotnet ef database update
- 5.Run the project:
  dotnet run
- 6.Open your browser and navigate to:
  https://localhost:{PORT}/swagger

🛠 Example Endpoints

- POST - Create Todo

  POST /api/todo
{
  "title": "Learn ASP.NET Core",
  "description": "Build web APIs using .NET",
  "dueDate": "2025-12-31T00:00:00",
  "priority": 2
}

- GET - All Todos

GET /api/todo

- GET - Todo by ID

GET /api/todo/{id}


✅ Features

Add, get, update, delete todo items

Input validation with ModelState

Error handling and logging

AutoMapper for DTOs

Swagger UI for interactive API testing

