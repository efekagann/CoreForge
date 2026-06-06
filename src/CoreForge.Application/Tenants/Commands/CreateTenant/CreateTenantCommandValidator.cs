using CoreForge.Application.Common.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CoreForge.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired])
            .MaximumLength(200).WithMessage(_ => localizer[ResourceKeys.Validation.FieldTooLong]);

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired])
            .MaximumLength(100).WithMessage(_ => localizer[ResourceKeys.Validation.FieldTooLong])
            .Matches(@"^[a-z0-9][a-z0-9\-]{1,98}[a-z0-9]$").WithMessage(_ => localizer[ResourceKeys.Validation.SlugInvalid]);

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage(_ => localizer[ResourceKeys.Validation.EmailInvalid])
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
