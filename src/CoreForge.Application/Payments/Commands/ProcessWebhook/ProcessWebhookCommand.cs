using MediatR;

namespace CoreForge.Application.Payments.Commands.ProcessWebhook;

public record ProcessWebhookCommand(string Payload, string Signature) : IRequest;
