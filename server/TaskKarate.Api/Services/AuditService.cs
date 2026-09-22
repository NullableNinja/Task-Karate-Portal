using System.Security.Claims;
using System.Text.Json;
using TaskKarate.Api.Data;
using TaskKarate.Api.Models;

namespace TaskKarate.Api.Services;

public sealed class AuditService(ApplicationDbContext db)
{
    public async Task RecordAsync(HttpContext context, string action, string entity, Guid entityId, object? metadata = null)
    {
        Guid? actor = Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        db.AuditEvents.Add(new AuditEvent { ActorUserId = actor, Action = action, Entity = entity, EntityId = entityId.ToString(), OccurredAtUtc = DateTime.UtcNow, MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata) });
        await db.SaveChangesAsync();
    }
}
