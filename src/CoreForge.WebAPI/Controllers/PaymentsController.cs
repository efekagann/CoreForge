using CoreForge.Application.Common.Interfaces;
using CoreForge.Application.Payments.Commands.CreateCheckoutSession;
using CoreForge.Application.Payments.Commands.ProcessWebhook;
using CoreForge.Application.Payments.DTOs;
using CoreForge.Application.Payments.Queries.GetCurrentSubscription;
using CoreForge.Application.Payments.Queries.GetPaymentHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreForge.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentsController(IMediator mediator, ITenantProvider tenantProvider) : ControllerBase
{
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCheckout(
        [FromBody] CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (tenantProvider.CurrentTenantId is null)
            return BadRequest("X-Tenant-Id header is required.");

        var command = new CreateCheckoutSessionCommand(
            tenantProvider.CurrentTenantId.Value,
            request.Plan,
            request.SuccessUrl,
            request.CancelUrl);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        string payload;
        using (var reader = new StreamReader(Request.Body))
            payload = await reader.ReadToEndAsync(cancellationToken);

        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;
        await mediator.Send(new ProcessWebhookCommand(payload, signature), cancellationToken);
        return Ok();
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(IList<PaymentTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        if (tenantProvider.CurrentTenantId is null)
            return BadRequest("X-Tenant-Id header is required.");

        var result = await mediator.Send(new GetPaymentHistoryQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("subscription")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscription(CancellationToken cancellationToken)
    {
        if (tenantProvider.CurrentTenantId is null)
            return BadRequest("X-Tenant-Id header is required.");

        var result = await mediator.Send(new GetCurrentSubscriptionQuery(), cancellationToken);
        return Ok(result);
    }
}
