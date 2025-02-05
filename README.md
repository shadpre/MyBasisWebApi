# MyBasisWebApi

MyBasisWebApi is a web API project built using ASP.NET Core. It includes features for user authentication, role management, and basic CRUD operations. The project is structured with a clean architecture, separating concerns into different layers such as BLL (Business Logic Layer) and DAL (Data Access Layer).

## Getting Started

To get started with MyBasisWebApi, follow the instructions below.

### Prerequisites

- .NET 6.0 SDK or later
- SQL Server
- Visual Studio or Visual Studio Code

### Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/shadpre/MyBasisWebApi.git
    cd MyBasisWebApi
    ```

2. Set up the database:
    - Update the connection string in `appsettings.json` to point to your SQL Server instance.
    - Run the following command to apply migrations and create the database:
        ```bash
        dotnet ef database update
        ```

3. Build the project:
    ```bash
    dotnet build
    ```

4. Run the project:
    ```bash
    dotnet run
    ```

## Usage

Once the project is running, you can access the API at `https://localhost:7000 or http://localhost:7100` (or the port specified in your launch settings).

## Project Structure

- **BLL**: Business Logic Layer containing DTOs, interfaces, repositories, and services.
- **DAL**: Data Access Layer containing entity configurations, context, and entities.
- **Controllers**: API controllers for handling HTTP requests.
- **Middleware**: Custom middleware for handling exceptions.

## Configuration

The project uses `appsettings.json` for configuration. Update the following settings as needed:

- **ConnectionStrings**: Update the `MyDbConnectionString` to point to your SQL Server instance.
- **JwtSettings**: Configure the JWT settings for authentication.

## API Endpoints

### Account

- **POST /api/Account/register**: Register a new user.
- **POST /api/Account/login**: Log in a user.
- **POST /api/Account/refreshtoken**: Refresh the authentication token.

### Other Endpoints

Add other endpoints as needed for your application.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any changes or improvements.

## License

This project is licensed under the MIT License.
