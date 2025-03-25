# To-Do List API

## Overview
This is a secure, feature-rich To-Do List API built with ASP.NET Core, featuring robust authentication, authorisation, and advanced filtering capabilities.

## Technology Stack
- **Framework:** ASP.NET Core 6.0  
- **Authentication:** JWT (JSON Web Tokens)  
- **Logging:** Serilog  
- **Data Storage:** JSON File-based Storage  
- **API Documentation:** Swagger  

## Prerequisites
- .NET 6.0 SDK  
- Visual Studio 2022 or Visual Studio Code  
- Postman (for API testing)  

## Project Structure
- `Controllers/` - API endpoint implementations  
- `Services/` - Business logic and data management  
- `Models/` - Data models and request/response structures  
- `Helpers/` - Utility classes for authentication and password management  

## Setup and Installation

### 1. Clone the Repository
```bash
git clone https://github.com/Alisha-suresh/ToDo.git
cd ToDo
```

### 2. Configure JWT Settings
Open `appsettings.json` and configure JWT settings:
```json
{
  "Jwt": {
    "Key": "YourSecretKeyHere",
    "Issuer": "TodoApiIssuer",
    "Audience": "TodoApiAudience"
  }
}
```

### 3. Run the Application
```bash
dotnet restore
dotnet run
```

The application will start at `http://localhost:5067`.

## Authentication Workflow

### User Roles
- **User:** Standard user with access to own todos.  
- **Admin:** Can manage and view all todos.  

### Authentication Endpoints
1. **Register:** `POST /api/auth/register`
   ```json
   {
     "username": "testuser",
     "password": "testuser123",
     "role": "User" 
   }
   ```

2. **Login:** `POST /api/auth/login`
   ```json
   {
     "username": "testuser",
     "password": "testuser123"
   }
   ```
   - Returns: `accessToken` and `refreshToken`

3. **Refresh Token:** `POST /api/auth/refresh-token`  
   - Renews access token using refresh token.  

## Todo API Endpoints

### Todo Management
1. **Create Todo:** `POST /api/todo`  
   - Requires authentication.  
   ```json
   {
     "title": "Grocery Shopping",
     "dueDate": "2024-03-30",
     "description": "Buy weekly groceries"
   }
   ```

2. **Get All Todos:** `GET /api/todo`  
   - Filters:
     - `completed` – Filter by completion status  
     - `dueDate` – Filter by specific date  
     - `sortBy` – Sort by (dueDate, title, creationDate, completed)  
     - `descending` – Reverse sort order  
     - `titleFilter` – Partial title search  

3. **Get Todo by ID:** `GET /api/todo/{id}`  

4. **Update Todo:** `PUT /api/todo/{id}`  

5. **Mark Todo Complete:** `PUT /api/todo/{id}/complete`  

6. **Delete Todo:** `DELETE /api/todo/{id}`  

### Admin-specific Endpoints
1. **Search Todos by User ID:** `GET /api/todo/admin/search?userId={username}`  
   - Admin-only endpoint to search todos for any user.  

## Authorisation Rules
- **Regular Users:**  
  - Can only manage their own todos.  
  - Limited filtering to personal tasks.  

- **Admin Users:**  
  - Can view, filter, and manage todos for all users.  
  - Can delete any todo.  

## Core Functionality
1. **User Authentication:** Secure login and registration with hashed passwords.  
2. **Role-Based Authorisation:** Different privileges for Users and Admins.  
3. **CRUD Operations:** Create, read, update, and delete todos with validation.  
4. **JWT Token Management:** Secure issuance, validation, and refresh.  
5. **Error Handling:** Consistent, meaningful error responses.  

## Advanced Features

### Search Scenarios
1. **User-Level Search**  
   - Users can search their own todos.  
   - Multiple filter criteria available:
     - Completion status  
     - Due date  
     - Partial title search  
     - Sorting options (ascending/descending)  

2. **Admin-Level Search**  
   - Admins can search todos for any user.  
   - Full access to all filtering and sorting options.  

### Deletion Scenarios
1. **User Deletion Rules**  
   - Users can only delete their own todos.  
   - Prevents unauthorised deletion.  

2. **Admin Deletion Capabilities**  
   - Admins can delete any todo.  
   - Useful for moderation or system cleanup.  

### Input Validation
- Ensures valid user input for registration, login, and todo creation.  
- Validates route parameters and query strings.  

### JWT Authentication
- Role-based Authorisation  
- Comprehensive Filtering  
- Secure Password Hashing  
- Refresh Token Mechanism  
- Detailed Error Handling  

## Design Choices
1. **WebAPI with RESTful Design:**  
   - Provides flexibility, scalability, and ease of integration with other systems.

2. **JWT Authentication:**  
   - Ensures secure, stateless authentication using tokens, minimizing server load.

3. **Role-based Access Control (RBAC):**  
   - Restricts actions based on user roles (User/Admin), enhancing security.

4. **File-based JSON Storage:**  
   - Simple and lightweight for managing small-scale data without requiring a database.

5. **Serilog for Logging:**  
   - Provides structured, detailed logging with minimal performance impact.

6. **Swagger for API Documentation:**  
   - Simplifies API exploration and testing for developers.

7. **Input Validation and Error Handling:**  
   - Prevents invalid data and ensures consistent error responses.

## Postman Collection Setup

### Step 1: Import Collection
1. Open Postman.  
2. Click "Import".  
3. Create a new collection named **"Todo API"**.  

### Recommended Testing Sequence
1. Register User.  
2. Login.  
3. Create Todos.  
4. Test Various Endpoints.  
5. Test Admin Functionalities (with admin user).  

## Security Considerations
- Passwords hashed before storage.  
- JWT tokens with expiration.  
- Role-based access control.  
- Secure token refresh mechanism.  
- Input validation to prevent malicious requests.  

## Logging
- Console logging.  
- File-based logging at `logs/todo-app.txt`.  
- Daily log rotation for efficient log management.  

## Potential Improvements
- Add comprehensive input validation.  
- Implement rate-limiting for enhanced security.  
- Create unit and integration tests for stability.  
- Introduce a database for larger-scale applications.  

## Troubleshooting
- Ensure .NET 6.0 SDK is installed.  
- Check JWT configuration.  
- Verify file permissions for JSON storage.
