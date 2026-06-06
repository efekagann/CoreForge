using MediatR;

namespace CoreForge.Application.Tenants.Commands.DeleteTenant;

public record DeleteTenantCommand(Guid Id) : IRequest;
