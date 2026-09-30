using CrmSupport.Api.Domain; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Identity.EntityFrameworkCore; using Microsoft.EntityFrameworkCore;
namespace CrmSupport.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> o) : IdentityDbContext<AppUser>(o) {
  public DbSet<Branch> Branches => Set<Branch>(); public DbSet<Department> Departments => Set<Department>();
  public DbSet<Customer> Customers => Set<Customer>(); public DbSet<CustomerNote> CustomerNotes => Set<CustomerNote>();
  public DbSet<Category> Categories => Set<Category>(); public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
  public DbSet<Ticket> Tickets => Set<Ticket>(); public DbSet<TicketComment> Comments => Set<TicketComment>(); public DbSet<TicketHistory> History => Set<TicketHistory>();
  public DbSet<KbArticle> Articles => Set<KbArticle>(); public DbSet<Feedback> Feedbacks => Set<Feedback>(); public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
}
public static class Seeder {
  public static async Task RunAsync(IServiceProvider sp) {
    var db = sp.GetRequiredService<AppDbContext>(); await db.Database.EnsureCreatedAsync();
    var rm = sp.GetRequiredService<RoleManager<IdentityRole>>(); var um = sp.GetRequiredService<UserManager<AppUser>>();
    foreach (var r in new[] { "Admin", "Manager", "Agent", "Customer" }) if (!await rm.RoleExistsAsync(r)) await rm.CreateAsync(new IdentityRole(r));
    foreach (var (email, role, pw) in new[] { ("admin@crm.local", "Admin", "Admin@123"), ("agent@crm.local", "Agent", "Agent@123") })
      if (await um.FindByEmailAsync(email) == null) { var u = new AppUser { UserName = email, Email = email, FullName = role }; await um.CreateAsync(u, pw); await um.AddToRoleAsync(u, role); }
    if (!db.SlaPolicies.Any()) db.SlaPolicies.AddRange(
      new SlaPolicy { Priority = Priority.Low, FirstResponseMinutes = 480, ResolutionMinutes = 4320 }, new SlaPolicy { Priority = Priority.Medium, FirstResponseMinutes = 240, ResolutionMinutes = 2880 },
      new SlaPolicy { Priority = Priority.High, FirstResponseMinutes = 60, ResolutionMinutes = 1440 }, new SlaPolicy { Priority = Priority.Urgent, FirstResponseMinutes = 15, ResolutionMinutes = 240 });
    if (!db.Categories.Any()) db.Categories.AddRange(new Category { NameEn = "Billing", NameAr = "الفواتير" }, new Category { NameEn = "Technical", NameAr = "الدعم الفني" }, new Category { NameEn = "General", NameAr = "عام" });
    await db.SaveChangesAsync();
  }
}
