# Quick Start Guide

## 🚀 Get Started in 5 Minutes

### Prerequisites Checklist
- [ ] .NET 8 SDK installed
- [ ] Docker Desktop running (or SQL Server locally)
- [ ] Git installed
- [ ] Your favorite IDE (VS Code, Visual Studio, Rider)

---

## Option 1: Docker (Recommended - Easiest)

### Step 1: Start Services
```bash
cd SaaS.MultiTenant
docker-compose up -d
```

This starts:
- ✅ SQL Server on port 1433
- ✅ Redis on port 6379
- ✅ API on port 5000

### Step 2: Access Swagger
Open browser: `http://localhost:5000/swagger`

### Step 3: Test API

**Register a new tenant:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@acme.com",
    "password": "SecurePass123!",
    "firstName": "John",
    "lastName": "Doe",
    "tenantName": "Acme Corp",
    "subdomain": "acme"
  }'
```

**Copy the JWT token from response, then:**
```bash
curl -X GET http://localhost:5000/api/v1/subscriptions/plans \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## Option 2: Local Development

### Step 1: Setup Database

**If using Docker for SQL Server only:**
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

**Or use your local SQL Server**

### Step 2: Update Connection String

Edit `src/SaaS.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your-Connection-String-Here"
  }
}
```

### Step 3: Apply Migrations

```bash
cd src/SaaS.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../SaaS.Api
dotnet ef database update --startup-project ../SaaS.Api
```

### Step 4: Run API

```bash
cd ../SaaS.Api
dotnet run
```

Access: `https://localhost:5001/swagger`

---

## Option 3: Visual Studio

1. Open `SaaS.MultiTenant.sln`
2. Set `SaaS.Api` as startup project
3. Update `appsettings.json` connection string
4. Press F5

---

## Testing the API

### Using Swagger UI
1. Go to `http://localhost:5000/swagger`
2. Try `/api/v1/auth/register` endpoint
3. Copy JWT token from response
4. Click "Authorize" button (top right)
5. Paste token in format: `Bearer YOUR_TOKEN`
6. Now test protected endpoints

### Using Postman
1. Import `postman_collection.json`
2. Run "Register New Tenant"
3. Token auto-saves to environment
4. Run other requests

### Using cURL
See examples in README.md "API Examples" section

---

## Verify Everything Works

### Check Database
```bash
# Connect to SQL Server
docker exec -it saas-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd"

# Run query
SELECT * FROM Tenants;
GO
```

### Check Redis
```bash
docker exec -it saas-redis redis-cli
> KEYS *
```

### Check Logs
```bash
# API logs
docker logs saas-api

# Or if running locally
tail -f logs/saas-*.txt
```

---

## Common Issues & Solutions

### Issue: Port 1433 already in use
**Solution**: Stop your local SQL Server or change port in docker-compose.yml

### Issue: "Cannot connect to database"
**Solution**: 
1. Check SQL Server is running: `docker ps`
2. Verify connection string in appsettings.json
3. Check firewall allows port 1433

### Issue: "Unauthorized" on all endpoints
**Solution**: 
1. Register first using `/api/v1/auth/register`
2. Copy token from response
3. Add to Authorization header: `Bearer {token}`

### Issue: Swagger not loading
**Solution**:
1. Ensure running in Development mode
2. Check `launchSettings.json` has `"ASPNETCORE_ENVIRONMENT": "Development"`

---

## Project Structure Overview

```
SaaS.MultiTenant/
├── src/
│   ├── SaaS.Domain/         ← Core entities (no dependencies)
│   ├── SaaS.Application/    ← Business logic (commands, queries)
│   ├── SaaS.Infrastructure/ ← Database, services, caching
│   └── SaaS.Api/            ← Controllers, middleware
├── tests/
│   └── SaaS.Tests/          ← Unit tests
├── docker-compose.yml       ← Docker orchestration
└── README.md                ← Full documentation
```

---

## Next Steps

### For Learning:
1. Read `ARCHITECTURE.md` for deep dive
2. Explore code comments
3. Run unit tests: `dotnet test`

---

## Sample Tenant Credentials (After Registration)

| Tenant | Email | Password | Subdomain |
|--------|-------|----------|-----------|
| Acme Corp | admin@acme.com | SecurePass123! | acme |
| TechStart | admin@techstart.com | SecurePass123! | techstart |

**Note**: These are created when you run register endpoint.

---

## Performance Monitoring

### Built-in Serilog
Logs saved to: `logs/saas-YYYYMMDD.txt`

### Manual Testing
```bash
# Install hey (load testing tool)
# macOS
brew install hey

# Linux
wget https://hey-release.s3.amazonaws.com/hey_linux_amd64

# Run load test
hey -n 1000 -c 10 http://localhost:5000/api/v1/subscriptions/plans
```

---

## Architecture Highlights

### Clean Architecture ✅
- Domain has zero dependencies
- Application depends only on Domain
- Infrastructure implements interfaces from Application

### Multi-Tenancy ✅
- Each request filtered by TenantId
- Global query filters prevent data leakage
- JWT contains tenant claim

### CQRS ✅
- Commands change state (CreateSubscription)
- Queries read state (GetSubscriptionPlans)
- MediatR orchestrates

---

## Where to Learn More

📖 **Full Documentation**: README.md
🏗️ **Architecture Details**: ARCHITECTURE.md
🧪 **API Testing**: postman_collection.json

---

## Support

**Questions?**
- Check README.md FAQ section
- Review code comments
- Open GitHub issue

**Found a bug?**
- Check existing issues
- Create detailed bug report
- Include logs and steps to reproduce

---

**🎉 You're ready to go! Start with Docker option for fastest setup.**

```bash
docker-compose up -d
# Wait 30 seconds for SQL Server to initialize
# Open http://localhost:5000/swagger
```
