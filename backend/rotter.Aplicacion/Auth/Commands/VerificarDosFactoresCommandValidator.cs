using FluentValidation;

namespace rotter.Aplicacion.Auth.Commands;

public class VerificarDosFactoresCommandValidator : AbstractValidator<VerificarDosFactoresCommand>
{
    public VerificarDosFactoresCommandValidator()
    {
        RuleFor(x => x.TokenDesafio).NotEmpty();
        RuleFor(x => x.Codigo).NotEmpty().Length(6).Matches("^[0-9]+$");
    }
}
