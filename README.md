I built a starter project: a .NET 8 Web API with an Angular 17 frontend (standalone components). I couldn't run dotnet or npm here, so it hasn't been compiled or tested. Expect to fix a few small compile errors on the first build.

To run it:

API: cd api && dotnet run --urls http://localhost:5000. It uses SQLite, creates the database on first run and seeds it. Swagger is at /swagger.
Web: cd web && npm install && npm start, then open http://localhost:4200. It proxies /api to the API.
Logins: admin@crm.local / Admin@123 and agent@crm.local / Agent@123
