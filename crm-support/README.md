# Support CRM (.NET 8 + Angular 17)
## Run
API:  cd api && dotnet run --urls http://localhost:5000   (Swagger at /swagger; SQLite db auto-created + seeded)
Web:  cd web && npm install && npm start                 (http://localhost:4200, proxies /api to :5000)
Logins: admin@crm.local / Admin@123 , agent@crm.local / Agent@123
Inbound channels: POST /api/inbound/{Email|WhatsApp|Sms|WebForm|LiveChat} with header X-Api-Key (see appsettings.json)
Before production: change Jwt:Key and Inbound:ApiKey, switch to UseSqlServer + EF migrations.
