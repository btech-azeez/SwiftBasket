# SwiftBasket

SwiftBasket is a grocery ordering demo with an Angular storefront and an ASP.NET Core API backed by SQL Server.

## Projects

- `Zepto.Frontend`: Angular 22 storefront and Admin dashboard.
- `Zepto.Backend`: ASP.NET Core 10 API, JWT authentication, and SQL Server data access.
- `Zepto.Backend/Database/SwiftBasket.sql`: complete SQL Server schema and idempotent demo master-data setup.

## Database

Run `Zepto.Backend/Database/SwiftBasket.sql` in SQL Server Management Studio or with `sqlcmd`. It creates `ZeptoDb` if needed, creates the tables and foreign keys, and seeds two demo users, eight categories, 24 products, and a sample order.

The demo credentials are:

- Admin: `admin@swiftbasket.com` / `Admin@123`
- Customer: `customer@swiftbasket.com` / `Customer@123`

Change these credentials before using the app beyond local development.

## Run Locally

The backend uses LocalDB by default. Configure a JWT signing secret of at least 32 bytes before starting it.

PowerShell:

```powershell
$env:JWT_SECRET = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet run --project .\Zepto.Backend\Zepto.Backend.csproj
```

Start the frontend in a second terminal:

```powershell
cd .\Zepto.Frontend
npm install
npm start
```

Open `http://localhost:4200`. The frontend API services currently target `http://localhost:5000`; use the backend HTTP launch profile to run the API on that port.

## Validate

```powershell
dotnet build .\Zepto.Backend\Zepto.Backend.csproj
cd .\Zepto.Frontend
npm run build
npm test -- --watch=false
```