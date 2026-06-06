using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Interfaces;
using CoreForge.Application.Payments.Commands.CreateCheckoutSession;
using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;

namespace CoreForge.Application.Tests.Payments;

public class CreateCheckoutSessionCommandHandlerTests
{
    private readonly IPaymentService _paymentService = Substitute.For<IPaymentService>();
    private readonly IRepository<Subscription> _subRepo = Substitute.For<IRepository<Subscription>>();
    private readonly IRepository<PaymentTransaction> _txnRepo = Substitute.For<IRepository<PaymentTransaction>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private CreateCheckoutSessionCommandHandler CreateHandler() =>
        new(_paymentService, _subRepo, _txnRepo, _uow);

    private static CreateCheckoutSessionCommand BuildCommand(Guid tenantId) =>
        new(tenantId, TenantPlan.Starter, "https://app.test/success", "https://app.test/cancel");

    [Fact]
    public async Task Handle_MockProvider_ImmediateActivation_CreatesSubscription()
    {
        var tenantId = Guid.NewGuid();
        _paymentService.ProviderName.Returns("Mock");
        _paymentService.CreateCheckoutSessionAsync(
            tenantId, TenantPlan.Starter, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new CheckoutSessionResult(true, "mock_session_001", null, false, null));

        _subRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Subscription, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(new List<Subscription>().AsReadOnly() as IReadOnlyList<Subscription>);

        var result = await CreateHandler().Handle(BuildCommand(tenantId), CancellationToken.None);

        Assert.False(result.RequiresRedirect);
        Assert.Equal("Mock", result.ProviderName);
        await _subRepo.Received(1).AddAsync(
            Arg.Is<Subscription>(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active),
            Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MockProvider_ExistingSubscription_UpdatesInsteadOfAdd()
    {
        var tenantId = Guid.NewGuid();
        var existing = new Subscription { TenantId = tenantId, Plan = TenantPlan.Free, Status = SubscriptionStatus.Trialing };

        _paymentService.ProviderName.Returns("Mock");
        _paymentService.CreateCheckoutSessionAsync(
            tenantId, TenantPlan.Starter, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new CheckoutSessionResult(true, "mock_002", null, false, null));

        _subRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Subscription, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(new List<Subscription> { existing }.AsReadOnly() as IReadOnlyList<Subscription>);

        await CreateHandler().Handle(BuildCommand(tenantId), CancellationToken.None);

        await _subRepo.DidNotReceive().AddAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
        _subRepo.Received(1).Update(Arg.Is<Subscription>(s => s.Status == SubscriptionStatus.Active));
    }

    [Fact]
    public async Task Handle_StripeProvider_RequiresRedirect_DoesNotCreateSubscription()
    {
        var tenantId = Guid.NewGuid();
        _paymentService.ProviderName.Returns("Stripe");
        _paymentService.CreateCheckoutSessionAsync(
            tenantId, TenantPlan.Starter, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new CheckoutSessionResult(true, "cs_stripe_001", "https://stripe.com/pay/xxx", true, null));

        var result = await CreateHandler().Handle(BuildCommand(tenantId), CancellationToken.None);

        Assert.True(result.RequiresRedirect);
        Assert.NotNull(result.CheckoutUrl);
        await _subRepo.DidNotReceive().AddAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProviderFailure_ThrowsConflictException()
    {
        var tenantId = Guid.NewGuid();
        _paymentService.ProviderName.Returns("Mock");
        _paymentService.CreateCheckoutSessionAsync(
            tenantId, TenantPlan.Starter, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new CheckoutSessionResult(false, string.Empty, null, false, "Provider error"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateHandler().Handle(BuildCommand(tenantId), CancellationToken.None));
    }
}
