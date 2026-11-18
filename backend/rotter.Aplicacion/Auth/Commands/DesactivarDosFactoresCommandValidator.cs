using FluentValidation;

namespace rotter.Aplicacion.Auth.Commands;

public class DesactivarDosFactoresCommandValidator : AbstractValidator<DesactivarDosFactoresCommand>
{
    public DesactivarDosFactoresCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().Length(6).Matches("^[0-9]+$");
    }
}
