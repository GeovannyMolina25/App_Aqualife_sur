using FluentValidation;

namespace rotter.Aplicacion.Auth.Commands;

public class RecuperarPasswordCommandValidator : AbstractValidator<RecuperarPasswordCommand>
{
    public RecuperarPasswordCommandValidator()
    {
        RuleFor(x => x.Dato).NotEmpty().MaximumLength(200);
    }
}
