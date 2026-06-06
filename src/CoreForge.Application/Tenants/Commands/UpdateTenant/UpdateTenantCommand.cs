using CoreForge.Application.Tenants.DTOs;
using CoreForge.Domain.Entities;
using MediatR;

namespace CoreForge.Application.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand(
    Guid Id,
    string Name,
    TenantPlan Plan,
    bool IsActive,
    string? ContactEmail
) : IRequest<TenantDto>;
