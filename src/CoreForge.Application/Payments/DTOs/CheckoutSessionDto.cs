namespace CoreForge.Application.Payments.DTOs;

public record CheckoutSessionDto(
    string SessionId,
    string? CheckoutUrl,
    bool RequiresRedirect,
    string ProviderName
);
