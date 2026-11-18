using FluentValidation;

namespace rotter.Aplicacion.Auth.Commands;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Dto.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Dto.Password).NotEmpty().MaximumLength(200);
    }
}
