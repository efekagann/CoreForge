using CoreForge.Application.Common.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CoreForge.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.EmailRequired])
            .EmailAddress().WithMessage(_ => localizer[ResourceKeys.Validation.EmailInvalid]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.PasswordRequired])
            .MinimumLength(8).WithMessage(_ => localizer[ResourceKeys.Validation.PasswordTooShort]);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired]);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.FieldRequired]);
    }
}
