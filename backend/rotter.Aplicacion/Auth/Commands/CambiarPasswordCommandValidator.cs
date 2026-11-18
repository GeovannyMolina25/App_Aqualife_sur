using FluentValidation;

namespace rotter.Aplicacion.Auth.Commands;

public class CambiarPasswordCommandValidator : AbstractValidator<CambiarPasswordCommand>
{
    public CambiarPasswordCommandValidator()
    {
        RuleFor(x => x.NuevaPassword)
            .NotEmpty()
            .MinimumLength(10)
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un dígito.")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un símbolo.");
    }
}
