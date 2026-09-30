using System.Text;
using CrmSupport.Api.Data; using CrmSupport.Api.Domain; using CrmSupport.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; using Microsoft.IdentityModel.Tokens;

var b = WebApplication.CreateBuilder(args);
b.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(b.Configuration.GetConnectionString("Default"))); // swap to UseSqlServer for production
b.Services.AddIdentityCore<AppUser>(o => o.Password.RequireNonAlphanumeric = false).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(b.Configuration["Jwt:Key"]!));
b.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(o => o.TokenValidationParameters = new() { IssuerSigningKey = key, ValidateIssuer = false, ValidateAudience = false });
b.Services.AddAuthorization();
b.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
b.Services.AddScoped<TicketService>();
b.Services.AddSingleton<IAiService, RuleBasedAi>();
b.Services.AddHostedService<SlaWorker>();
b.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
b.Services.AddEndpointsApiExplorer(); b.Services.AddSwaggerGen();

var app = b.Build();
app.UseCors(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
using (var s = app.Services.CreateScope()) await Seeder.RunAsync(s.ServiceProvider);
app.Run();
