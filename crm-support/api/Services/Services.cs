using CrmSupport.Api.Data; using CrmSupport.Api.Domain; using Microsoft.AspNetCore.Identity; using Microsoft.EntityFrameworkCore;
namespace CrmSupport.Api.Services;
public record NewTicket(string Subject, string Description, int CustomerId, int? CategoryId, Priority Priority, Channel Channel);

public class TicketService(AppDbContext db, UserManager<AppUser> um) {
  public async Task<Ticket> CreateAsync(NewTicket n, string? actor) {
    var sla = await db.SlaPolicies.FirstOrDefaultAsync(s => s.Priority == n.Priority); var now = DateTime.UtcNow;
    var t = new Ticket { Number = $"T-{now:yyMMdd}-{Random.Shared.Next(1000, 9999)}", Subject = n.Subject, Description = n.Description, CustomerId = n.CustomerId, CategoryId = n.CategoryId,
      Priority = n.Priority, Channel = n.Channel, ResponseDue = now.AddMinutes(sla?.FirstResponseMinutes ?? 240), ResolutionDue = now.AddMinutes(sla?.ResolutionMinutes ?? 2880) };
    // Auto-assignment: agent with the fewest open tickets
    var agents = await um.GetUsersInRoleAsync("Agent");
    var load = await db.Tickets.Where(x => x.Status != TicketStatus.Resolved && x.Status != TicketStatus.Closed && x.AssigneeId != null).GroupBy(x => x.AssigneeId).Select(g => new { g.Key, C = g.Count() }).ToDictionaryAsync(x => x.Key!, x => x.C);
    t.AssigneeId = agents.OrderBy(a => load.GetValueOrDefault(a.Id)).FirstOrDefault()?.Id;
    if (t.AssigneeId != null) t.Status = TicketStatus.Open;
    t.History.Add(new TicketHistory { Action = $"Created via {n.Channel}", Actor = actor });
    db.Tickets.Add(t); await db.SaveChangesAsync(); return t;
  }
}

public interface IAiService { string Summarize(Ticket t); string SuggestReply(Ticket t); string Categorize(string text); }
// Placeholder logic. Replace with an LLM call (Anthropic/Azure OpenAI) behind the same interface.
public class RuleBasedAi : IAiService {
  public string Summarize(Ticket t) => $"[{t.Priority}] {t.Subject} — {t.Comments.Count} comment(s), status {t.Status}.";
  public string SuggestReply(Ticket t) => $"Hello, thank you for contacting us about \"{t.Subject}\". We are looking into it and will update you shortly.";
  public string Categorize(string x) { x = x.ToLower(); return x.Contains("invoice") || x.Contains("payment") || x.Contains("فاتورة") ? "Billing" : x.Contains("error") || x.Contains("login") || x.Contains("خطأ") ? "Technical" : "General"; }
}

public class SlaWorker(IServiceScopeFactory f, ILogger<SlaWorker> log) : BackgroundService {
  protected override async Task ExecuteAsync(CancellationToken ct) {
    while (!ct.IsCancellationRequested) {
      using var s = f.CreateScope(); var db = s.ServiceProvider.GetRequiredService<AppDbContext>(); var now = DateTime.UtcNow;
      var late = await db.Tickets.Where(t => !t.Breached && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed &&
        ((t.FirstResponseAt == null && t.ResponseDue < now) || t.ResolutionDue < now)).ToListAsync(ct);
      foreach (var t in late) { t.Breached = true; t.Status = TicketStatus.Escalated; t.History.Add(new TicketHistory { Action = "SLA breached - auto escalated", Actor = "system" }); log.LogWarning("SLA breach {n}", t.Number); /* TODO: email/SMS alert */ }
      if (late.Count > 0) await db.SaveChangesAsync(ct);
      await Task.Delay(TimeSpan.FromMinutes(1), ct);
    }
  }
}
