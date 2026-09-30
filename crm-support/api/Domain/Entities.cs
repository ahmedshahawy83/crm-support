using Microsoft.AspNetCore.Identity;
namespace CrmSupport.Api.Domain;
public enum Channel { Email, WhatsApp, LiveChat, Sms, WebForm, Portal }
public enum Priority { Low, Medium, High, Urgent }
public enum TicketStatus { New, Open, Pending, Escalated, Resolved, Closed }
public class Branch { public int Id { get; set; } public string NameEn { get; set; } = ""; public string NameAr { get; set; } = ""; }
public class Department { public int Id { get; set; } public string NameEn { get; set; } = ""; public string NameAr { get; set; } = ""; public int? BranchId { get; set; } }
public class AppUser : IdentityUser { public string FullName { get; set; } = ""; public int? DepartmentId { get; set; } public int? BranchId { get; set; } public string Lang { get; set; } = "en"; }
public class Customer { public int Id { get; set; } public string Name { get; set; } = ""; public string? Email { get; set; } public string? Phone { get; set; } public string? Company { get; set; } public int? BranchId { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public List<CustomerNote> Notes { get; set; } = new(); public List<Ticket> Tickets { get; set; } = new(); }
public class CustomerNote { public int Id { get; set; } public int CustomerId { get; set; } public string Text { get; set; } = ""; public string? Author { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; }
public class Category { public int Id { get; set; } public string NameEn { get; set; } = ""; public string NameAr { get; set; } = ""; }
public class SlaPolicy { public int Id { get; set; } public Priority Priority { get; set; } public int FirstResponseMinutes { get; set; } public int ResolutionMinutes { get; set; } }
public class Ticket {
  public int Id { get; set; } public string Number { get; set; } = ""; public string Subject { get; set; } = ""; public string Description { get; set; } = "";
  public int CustomerId { get; set; } public Customer? Customer { get; set; } public int? CategoryId { get; set; } public int? DepartmentId { get; set; }
  public Priority Priority { get; set; } public TicketStatus Status { get; set; } = TicketStatus.New; public Channel Channel { get; set; }
  public string? AssigneeId { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? FirstResponseAt { get; set; } public DateTime? ResolvedAt { get; set; } public DateTime ResponseDue { get; set; } public DateTime ResolutionDue { get; set; } public bool Breached { get; set; }
  public List<TicketComment> Comments { get; set; } = new(); public List<TicketHistory> History { get; set; } = new();
}
public class TicketComment { public int Id { get; set; } public int TicketId { get; set; } public string? Author { get; set; } public string Body { get; set; } = ""; public bool IsInternal { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; }
public class TicketHistory { public int Id { get; set; } public int TicketId { get; set; } public string Action { get; set; } = ""; public string? Actor { get; set; } public DateTime At { get; set; } = DateTime.UtcNow; }
public class KbArticle { public int Id { get; set; } public string TitleEn { get; set; } = ""; public string TitleAr { get; set; } = ""; public string BodyEn { get; set; } = ""; public string BodyAr { get; set; } = ""; public bool Published { get; set; } = true; }
public class Feedback { public int Id { get; set; } public int TicketId { get; set; } public int Rating { get; set; } public string? Comment { get; set; } }
public class AuditLog { public int Id { get; set; } public string? User { get; set; } public string Action { get; set; } = ""; public string Entity { get; set; } = ""; public DateTime At { get; set; } = DateTime.UtcNow; }
