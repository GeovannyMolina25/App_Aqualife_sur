using FluentValidation;

namespace rotter.Aplicacion.Auth.Commands;

public class ConfirmarDosFactoresCommandValidator : AbstractValidator<ConfirmarDosFactoresCommand>
{
    public ConfirmarDosFactoresCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().Length(6).Matches("^[0-9]+$");
    }
}
