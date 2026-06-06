using CoreForge.Domain.Entities;

namespace CoreForge.Application.Tenants.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    TenantPlan Plan,
    bool IsActive,
    string? ContactEmail,
    DateTime CreatedAt
);
