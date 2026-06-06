using CoreForge.Application.Common.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CoreForge.Application.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.EmailRequired])
            .EmailAddress().WithMessage(_ => localizer[ResourceKeys.Validation.EmailInvalid]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_ => localizer[ResourceKeys.Validation.PasswordRequired]);
    }
}
