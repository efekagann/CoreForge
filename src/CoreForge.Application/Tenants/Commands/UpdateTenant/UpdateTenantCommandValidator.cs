using CoreForge.Application.Common.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CoreForge.Application.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired])
            .MaximumLength(200).WithMessage(_ => localizer[ResourceKeys.Validation.FieldTooLong]);

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage(_ => localizer[ResourceKeys.Validation.EmailInvalid])
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
