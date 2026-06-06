using CoreForge.Application.Common.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CoreForge.Application.Payments.Commands.CreateCheckoutSession;

public class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired]);

        RuleFor(x => x.SuccessUrl)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired]);

        RuleFor(x => x.CancelUrl)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired]);
    }
}
