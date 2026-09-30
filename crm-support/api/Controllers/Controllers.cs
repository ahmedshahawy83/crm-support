using System.IdentityModel.Tokens.Jwt; using System.Security.Claims; using System.Text;
using CrmSupport.Api.Data; using CrmSupport.Api.Domain; using CrmSupport.Api.Services;
using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using Microsoft.IdentityModel.Tokens;
namespace CrmSupport.Api.Controllers;

public record LoginDto(string Email, string Password);
[ApiController, Route("api/auth")]
public class AuthController(UserManager<AppUser> um, IConfiguration cfg) : ControllerBase {
  [HttpPost("login")] public async Task<IActionResult> Login(LoginDto d) {
    var u = await um.FindByEmailAsync(d.Email); if (u == null || !await um.CheckPasswordAsync(u, d.Password)) return Unauthorized();
    var role = (await um.GetRolesAsync(u)).FirstOrDefault() ?? "Customer";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, u.Id), new Claim(ClaimTypes.Name, u.Email!), new Claim(ClaimTypes.Role, role) };
    var cred = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: cred));
    return Ok(new { token, role, name = u.FullName });
  }
}

[ApiController, Route("api/admin"), Authorize(Roles = "Admin")]
public class AdminController(UserManager<AppUser> um, AppDbContext db) : ControllerBase {
  public record NewUser(string Email, string FullName, string Password, string Role);
  [HttpPost("users")] public async Task<IActionResult> Create(NewUser n) {
    var u = new AppUser { UserName = n.Email, Email = n.Email, FullName = n.FullName }; var r = await um.CreateAsync(u, n.Password);
    if (!r.Succeeded) return BadRequest(r.Errors); await um.AddToRoleAsync(u, n.Role);
    db.AuditLogs.Add(new AuditLog { User = User.Identity!.Name, Action = "CreateUser", Entity = n.Email }); await db.SaveChangesAsync(); return Ok();
  }
  [HttpGet("audit")] public async Task<IActionResult> Audit() => Ok(await db.AuditLogs.OrderByDescending(a => a.Id).Take(200).ToListAsync());
}

[ApiController, Route("api/customers"), Authorize(Roles = "Admin,Manager,Agent")]
public class CustomersController(AppDbContext db) : ControllerBase {
  [HttpGet] public async Task<IActionResult> List(string? q) => Ok(await db.Customers.Where(c => q == null || c.Name.Contains(q) || (c.Email ?? "").Contains(q) || (c.Phone ?? "").Contains(q)).OrderBy(c => c.Name).Take(100).ToListAsync());
  [HttpGet("{id}")] public async Task<IActionResult> Get(int id) => await db.Customers.Include(c => c.Notes).Include(c => c.Tickets).FirstOrDefaultAsync(c => c.Id == id) is { } c ? Ok(c) : NotFound();
  [HttpPost] public async Task<IActionResult> Create(Customer c) { c.Id = 0; db.Customers.Add(c); await db.SaveChangesAsync(); return Ok(c); }
  [HttpPost("{id}/notes")] public async Task<IActionResult> Note(int id, CustomerNote n) { n.Id = 0; n.CustomerId = id; n.Author = User.Identity!.Name; db.CustomerNotes.Add(n); await db.SaveChangesAsync(); return Ok(n); }
}

[ApiController, Route("api/tickets"), Authorize]
public class TicketsController(AppDbContext db, TicketService svc, IAiService ai) : ControllerBase {
  bool Staff => !User.IsInRole("Customer");
  [HttpGet] public async Task<IActionResult> List(TicketStatus? status, bool mine = false) {
    var q = db.Tickets.Include(t => t.Customer).AsQueryable();
    if (!Staff) q = q.Where(t => t.Customer!.Email == User.Identity!.Name); // customer portal: own tickets only
    if (mine) q = q.Where(t => t.AssigneeId == User.FindFirstValue(ClaimTypes.NameIdentifier));
    if (status != null) q = q.Where(t => t.Status == status);
    return Ok(await q.OrderByDescending(t => t.CreatedAt).Take(200).Select(t => new { t.Id, t.Number, t.Subject, t.Priority, t.Status, t.Channel, t.Breached, t.CreatedAt, t.ResolutionDue, customer = t.Customer!.Name }).ToListAsync());
  }
  [HttpGet("{id}")] public async Task<IActionResult> Get(int id) {
    var t = await db.Tickets.Include(x => x.Customer).Include(x => x.Comments).Include(x => x.History).FirstOrDefaultAsync(x => x.Id == id); if (t == null) return NotFound();
    if (!Staff) { if (t.Customer?.Email != User.Identity!.Name) return Forbid(); t.Comments = t.Comments.Where(c => !c.IsInternal).ToList(); }
    return Ok(t);
  }
  [HttpPost] public async Task<IActionResult> Create(NewTicket n) => Ok(await svc.CreateAsync(n, User.Identity!.Name));
  public record CommentDto(string Body, bool IsInternal);
  [HttpPost("{id}/comments")] public async Task<IActionResult> Comment(int id, CommentDto d) {
    var t = await db.Tickets.FindAsync(id); if (t == null) return NotFound();
    db.Comments.Add(new TicketComment { TicketId = id, Body = d.Body, IsInternal = Staff && d.IsInternal, Author = User.Identity!.Name });
    if (Staff && !d.IsInternal) t.FirstResponseAt ??= DateTime.UtcNow; await db.SaveChangesAsync(); return Ok();
  }
  public record StatusDto(TicketStatus Status); public record AssignDto(string AgentId);
  [HttpPut("{id}/status"), Authorize(Roles = "Admin,Manager,Agent")] public async Task<IActionResult> Status(int id, StatusDto d) {
    var t = await db.Tickets.FindAsync(id); if (t == null) return NotFound();
    t.Status = d.Status; if (d.Status == TicketStatus.Resolved) t.ResolvedAt = DateTime.UtcNow;
    db.History.Add(new TicketHistory { TicketId = id, Action = $"Status -> {d.Status}", Actor = User.Identity!.Name }); await db.SaveChangesAsync(); return Ok();
  }
  [HttpPut("{id}/assign"), Authorize(Roles = "Admin,Manager")] public async Task<IActionResult> Assign(int id, AssignDto d) {
    var t = await db.Tickets.FindAsync(id); if (t == null) return NotFound(); t.AssigneeId = d.AgentId;
    db.History.Add(new TicketHistory { TicketId = id, Action = "Reassigned", Actor = User.Identity!.Name }); await db.SaveChangesAsync(); return Ok();
  }
  [HttpGet("{id}/ai"), Authorize(Roles = "Admin,Manager,Agent")] public async Task<IActionResult> Ai(int id) {
    var t = await db.Tickets.Include(x => x.Comments).FirstOrDefaultAsync(x => x.Id == id); if (t == null) return NotFound();
    return Ok(new { summary = ai.Summarize(t), suggestedReply = ai.SuggestReply(t), category = ai.Categorize(t.Subject + " " + t.Description) });
  }
  public record FeedbackDto(int Rating, string? Comment);
  [HttpPost("{id}/feedback")] public async Task<IActionResult> Feedback(int id, FeedbackDto d) { db.Feedbacks.Add(new Feedback { TicketId = id, Rating = Math.Clamp(d.Rating, 1, 5), Comment = d.Comment }); await db.SaveChangesAsync(); return Ok(); }
}

[ApiController, Route("api/kb")]
public class KbController(AppDbContext db) : ControllerBase {
  [HttpGet, AllowAnonymous] public async Task<IActionResult> Search(string? q) => Ok(await db.Articles.Where(a => a.Published && (q == null || a.TitleEn.Contains(q) || a.TitleAr.Contains(q) || a.BodyEn.Contains(q) || a.BodyAr.Contains(q))).ToListAsync());
  [HttpPost, Authorize(Roles = "Admin,Manager,Agent")] public async Task<IActionResult> Create(KbArticle a) { a.Id = 0; db.Articles.Add(a); await db.SaveChangesAsync(); return Ok(a); }
}

[ApiController, Route("api/reports"), Authorize(Roles = "Admin,Manager")]
public class ReportsController(AppDbContext db) : ControllerBase {
  [HttpGet("summary")] public async Task<IActionResult> Summary() => Ok(new {
    total = await db.Tickets.CountAsync(), open = await db.Tickets.CountAsync(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed),
    breached = await db.Tickets.CountAsync(t => t.Breached), avgRating = await db.Feedbacks.Select(f => (double?)f.Rating).AverageAsync() ?? 0,
    byStatus = await db.Tickets.GroupBy(t => t.Status).Select(g => new { status = g.Key, count = g.Count() }).ToListAsync(),
    byAgent = await db.Tickets.Where(t => t.AssigneeId != null).GroupBy(t => t.AssigneeId).Select(g => new { agent = g.Key, count = g.Count() }).ToListAsync() });
}

// Webhook for Email / WhatsApp / SMS / Web-form providers. Header: X-Api-Key
[ApiController, Route("api/inbound")]
public class InboundController(AppDbContext db, TicketService svc, IConfiguration cfg) : ControllerBase {
  public record InboundMsg(string Name, string? Email, string? Phone, string Subject, string Body);
  [HttpPost("{channel}"), AllowAnonymous] public async Task<IActionResult> Receive(Channel channel, InboundMsg m) {
    if (Request.Headers["X-Api-Key"] != cfg["Inbound:ApiKey"]) return Unauthorized();
    var c = await db.Customers.FirstOrDefaultAsync(x => (m.Email != null && x.Email == m.Email) || (m.Phone != null && x.Phone == m.Phone));
    if (c == null) { c = new Customer { Name = m.Name, Email = m.Email, Phone = m.Phone }; db.Customers.Add(c); await db.SaveChangesAsync(); }
    var t = await svc.CreateAsync(new NewTicket(m.Subject, m.Body, c.Id, null, Priority.Medium, channel), "inbound"); return Ok(new { t.Number });
  }
}
