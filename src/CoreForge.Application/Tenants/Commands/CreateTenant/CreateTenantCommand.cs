using CoreForge.Application.Tenants.DTOs;
using CoreForge.Domain.Entities;
using MediatR;

namespace CoreForge.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(
    string Name,
    string Slug,
    TenantPlan Plan,
    string? ContactEmail
) : IRequest<TenantDto>;
