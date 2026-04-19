# SecureVaultApp

An ASP.NET Core MVC application demonstrating secure document storage with encryption, JWT authentication, role-based access control, and audit logging.

## Project Phases

| Phase | Description |
|-------|-------------|
| Phase 1 | Project setup — ASP.NET Core MVC scaffolding, folder structure |
| Phase 2 | Data layer — Models, DTOs, ViewModels, DbContext, database seeder |
| Phase 3 | Services layer — Encryption services, JWT token service, audit service, masking helper |

## Architecture Overview

```
SecureVaultApp/
├── Controllers/          # MVC controllers
├── Data/                 # DbContext + seeder
├── DTOs/                 # Request/response data transfer objects
├── Helpers/              # MaskingHelper (PII masking)
├── Interfaces/           # IEncryptionService, ITokenService
├── Models/               # EF Core entity models
├── Services/
│   ├── EncryptionServices/
│   │   ├── AesEncryptionService.cs        # AES-256 + HMAC-SHA256
│   │   ├── DataProtectionEncryptionService.cs  # ASP.NET Data Protection
│   │   └── RsaEncryptionService.cs        # RSA-OAEP-SHA256
│   ├── TokenService.cs   # JWT + refresh token generation
│   └── AuditService.cs   # Audit log persistence
└── ViewModels/           # Login/Register view models
```

## Configuration

### 1. appsettings.json

Add the following sections (never commit real secrets — use User Secrets or environment variables in production):

```json
{
  "JwtSettings": {
    "Secret": "YOUR-256-BIT-BASE64-SECRET-HERE",
    "Issuer": "SecureVaultApp",
    "Audience": "SecureVaultClient",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "DataProtection": {
    "EncryptionKey": "YOUR-BASE64-AES-256-KEY-HERE"
  }
}
```

#### Generating a JWT Secret (PowerShell)
```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Max 256 }))
```

#### Generating an AES-256 Key (PowerShell)
```powershell
$key = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Fill($key)
[Convert]::ToBase64String($key)
```

### 2. User Secrets (recommended for local development)

```bash
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "<your-secret>"
dotnet user-secrets set "DataProtection:EncryptionKey" "<your-aes-key>"
```

### 3. Active Encryption Service

`Program.cs` registers one `IEncryptionService` at a time. Switch by changing the registration:

```csharp
// Option A — ASP.NET Data Protection (default, no key config needed)
builder.Services.AddDataProtection();
builder.Services.AddScoped<IEncryptionService, DataProtectionEncryptionService>();

// Option B — AES-256 + HMAC (requires DataProtection:EncryptionKey in config)
builder.Services.AddScoped<IEncryptionService, AesEncryptionService>();

// Option C — RSA (requires DataProtection:RsaPublicKey + RsaPrivateKey PEM strings in config)
builder.Services.AddScoped<IEncryptionService, RsaEncryptionService>();
```

## Running the App

```bash
# Restore dependencies
dotnet restore

# Run (uses in-memory database, no setup needed)
dotnet run
```

The app seeds an admin user on first run via `DbSeeder`. Check `Data/DbSeeder.cs` for the default credentials.

## Notes

- The database is **in-memory** — all data is lost on restart. Swap `UseInMemoryDatabase` for a real provider (SQL Server, SQLite) for persistence.
- JWT tokens are stateless. Refresh tokens are stored in the database.
- All encryption services implement `IEncryptionService` — swap freely via DI without changing any other code.
