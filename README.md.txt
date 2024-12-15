# FinalProjectV3

A Web API for managing loans, built with ASP.NET Core.

## Table of Contents
1. [Installation](#installation)
2. [Usage](#usage)
3. [Technologies Used](#technologies-used)
4. [Contributing](#contributing)
5. [License](#license)

## Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/your-username/FinalProjectV3.git
    ```

2. Navigate into the project directory:
    ```bash
    cd FinalProjectV3
    ```

3. Restore dependencies:
    ```bash
    dotnet restore
    ```

4. Run the project:
    ```bash
    dotnet run
    ```

## Usage

To interact with the Loan API, you can use tools like Postman or cURL.

### Authentication
This API uses **JWT authentication**. To access protected endpoints, you need to obtain a JWT token through the login endpoint and then use it for authorization.

- **Login to get a token**:
    ```bash
    POST /api/auth/login
    {
        "username": "user@example.com",
        "password": "password"
    }
    ```

- **Using the token for authorized requests**:
    Include the token in the `Authorization` header:
    ```bash
    Authorization: Bearer <your-jwt-token>
    ```

### API Endpoints

- **Get all loans**:
    ```bash
    GET /api/loans
    ```

- **Create a new loan**:
    ```bash
    POST /api/loans
    {
        "amount": 10000,
        "interestRate": 5.5
    }
    ```

## Technologies Used

- **ASP.NET Core**: Framework for building the Web API.
- **Entity Framework Core (EF Core)**: ORM for database interaction with SQL Server.
- **FluentValidation**: For input validation, such as validating DTOs (Data Transfer Objects).
- **Swagger (Swashbuckle.AspNetCore)**: For API documentation and testing.
- **JWT Authentication**: Using `Microsoft.AspNetCore.Authentication.JwtBearer` for secure, token-based authentication.
- **NLog**: For logging via `NLog.Extensions.Logging` and `NLog.Web.AspNetCore`.
- **Microsoft.AspNetCore.Identity**: For managing user authentication, authorization, and roles.

## Key Services & Middleware

- **Authentication & Authorization**: The application uses JWT authentication, configured with the `JwtBearerDefaults.AuthenticationScheme` and token validation parameters.
- **Custom Middleware**: 
  - **`UnauthorizedResponseMiddleware`**: Handles unauthorized responses.
  - **`ForbiddenMiddleware`**: Handles forbidden responses.
- **DbContext**: The application uses `AppDbContext` to interact with the SQL Server database, configured to use a connection string from the `appsettings.json`.

## Contributing

If you'd like to contribute to this project, follow these steps:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-name`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push to your branch (`git push origin feature-name`).
5. Create a new Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Explanation:

1. **Registration**: Describes how users and accountants can register, including the necessary details (username, password, email for users, etc.).
2. **Login**: Describes the login process where users and accountants authenticate, and the API responds with a JWT token.
3. **JWT Token**: It highlights that once the user logs in, a JWT token is returned, which must be used for subsequent authenticated requests.
4. **Error Handling**: The README explains the expected behavior when validation or authentication fails.

This section ensures that developers and users of your API understand how to interact with the authentication system properly.
## Authentication

The API supports user authentication through JWT (JSON Web Tokens). Below are the available endpoints for user and accountant registration, as well as login.

### Authentication Endpoints

#### 1. Register a User
- **Endpoint**: `POST /api/auth/register`
- **Request Body**:
    ```json
    {
        "username": "user123",
        "password": "password123",
        "firstName": "John",
        "lastName": "Doe",
        "email": "user@example.com",
        "age": 30,
        "salary": 50000
    }
    ```
- **Response**:
    ```json
    {
        "userId": 1,
        "firstName": "John",
        "lastName": "Doe",
        "email": "user@example.com",
        "username": "user123",
        "age": 30,
        "salary": 50000
    }
    ```
- **Description**: This endpoint registers a new user. It checks for duplicate usernames and emails and securely stores the user’s password using HMAC-SHA512.

#### 2. Register an Accountant
- **Endpoint**: `POST /api/auth/register-accountant`
- **Request Body**:
    ```json
    {
        "username": "accountant123",
        "password": "password123",
        "firstName": "Jane",
        "lastName": "Smith"
    }
    ```
- **Response**:
    ```json
    {
        "accountantId": 1,
        "firstName": "Jane",
        "lastName": "Smith",
        "username": "accountant123"
    }
    ```
- **Description**: This endpoint registers a new accountant. It checks for duplicate usernames and securely stores the accountant’s password.

#### 3. Login
- **Endpoint**: `POST /api/auth/login`
- **Request Body**:
    ```json
    {
        "username": "user123",
        "password": "password123"
    }
    ```
- **Response**:
    ```json
    {
        "token": "your-jwt-token-here"
    }
    ```
- **Description**: This endpoint logs in the user or accountant. Upon successful login, it generates a JWT token that must be included in the Authorization header for accessing protected routes.

    Example:
    ```bash
    Authorization: Bearer <your-jwt-token-here>
    ```

### Error Handling

- **Invalid Input / Validation Failure**: If the registration or login request contains invalid data, a `400 Bad Request` will be returned.
- **Unauthorized**: If the username or password is incorrect, a `401 Unauthorized` response will be returned with an error message.
- **Server Errors**: If there’s an unexpected error during registration or login, a `500 Internal Server Error` will be returned.

### JWT Token Usage

After a successful login, you will receive a JWT token. This token should be included in the `Authorization` header for all requests to protected endpoints.

Example:
```bash
Authorization: Bearer <your-jwt-token-here>

###  Accountant Service Endpoints

#### 1. ViewAllUsers
- **Endpoint**: `GET /api/Accountant/view-users`
- **Request Body**:  No need
- **Response**:
    [
    {
        "userId": 1,
        "firstName": "John",
        "lastName": "Doe",
        "email": "user@example.com",
        "age": 30,
        "salary": 50000,
        "username": "user123"
    },
    ...
]
    ```
#### 2. ViewAllLoanRequest
- **Endpoint**: `GET /api/accountant/view-loan-requests`
- **Request Body**:  No need
- **Response**:
    [
    {
        "loanId": 1,
        "loanType": "Personal",
        "amount": 10000,
        "currency": "USD",
        "period": 12,
        "loanStatus": "Pending"
    },
    ...
]
#### 3. Block or Unblock a User
- **Endpoint**: `PATCH /api/accountant/block-or-unblock-user/{userId}`
- **Request Body**:  userId isBlocked(true/false)
In postman(url/api/accountant/block-or-unblock-user/{userId}?isBlocked=true/false) 
- **Response**:
{
    "message": "User successfully blocked."
}
]
#### 4. Change Loan Status
- **Endpoint**: `Patch /api/accountant/change-loan-status/{userId}/{loanId}`
- **Request Body**: userId loanId newstatus (0, 1, 2)
In postman(url/api/accountant/change-loan-status/{userId}/{loanId}?newStatus=0/1/2) 
- **Response**:
    {
    "message": "Loan status successfully changed."
}
#### 5. Delete a Loan
- **Endpoint**: `DELETE /api/accountant/delete-loan/{loanId}`
- **Request Body**: loanId  
- **Response**:
    {
    "message": "Loan successfully deleted."
}
### User Service Endpoints
#### 1.Add Loan Request
- **Endpoint**: ` POST /api/user/loan-request
- **Request Body**:
{
  "loanType": "Auto",
  "amount": 10000,
  "currency": "USD",
  "period": Sixmonth
}
 
- **Response**:
Success:
Status: 200 OK
Body: The loan request details including LoanId, LoanType, Amount, Currency, Period, and LoanStatus.
Error:
ValidationException: Returns 400 Bad Request with the validation error message.
UnauthorizedAccessException: Returns 403 Forbidden if the logged-in user is unauthorized.
#### 2.View User Cabinet
- **Endpoint**: ` GET /api/user/user-cabinet/{userId}
- **Request Body**: userId
- **Response**:
Success:
Status: 200 OK
Body: The user details, including FirstName, LastName, Email, Age, Salary, and Username.
Error:
UnauthorizedAccessException: Returns 403 Forbidden if the logged-in user is unauthorized to view the information.


#### 3.View All loan History
- **Endpoint**: ` GET /api/user/{userId}/view-loans-history
- **Request Body**: userId
- **Response**:
Success:
Status: 200 OK
Body: A list of loan request responses.
Error:
UnauthorizedAccessException: Returns 403 Forbidden if the logged-in user is unauthorized.
KeyNotFoundException: Returns 404 Not Found if no loans are found for the user.
#### 4.Update Loan
- **Endpoint**: ` PUT /api/user/users/{userId}/loans/{loanId}/update-loan
- **Request Body**: userId loanId
- **Response**:
{
  "loanType": "fast",
  "amount": 12000,
  "currency": "USD",
  "period": threemonth
}
Success:
Status: 200 OK
Body: The updated loan details.
Error:
UnauthorizedAccessException: Returns 403 Forbidden if the logged-in user is unauthorized to update the loan.
InvalidOperationException: Returns 409 Conflict if the loan status is not "InProgress" (only loans in progress can be updated).
KeyNotFoundException: Returns 404 Not Found if the loan is not found.
#### 5.Delete Loan
- **Endpoint**: ` DELETE /api/user/users/{userId}/loans/{loanId}/delete-loan
- **Request Body**: userId loanId
- **Response**:
       Success:
Status: 200 OK
Body: Message confirming loan deletion.
Error:
UnauthorizedAccessException: Returns 403 Forbidden if the logged-in user is unauthorized to delete the loan.
KeyNotFoundException: Returns 404 Not Found if the loan is not found.
InvalidOperationException: Returns 400 Bad Request if the loan status is not "InProgress" (only loans in progress can be deleted).




