using FluentValidation;

namespace rotter.Aplicacion.Usuarios.Commands;

public class CrearUsuarioRapidoCommandValidator : AbstractValidator<CrearUsuarioRapidoCommand>
{
    public CrearUsuarioRapidoCommandValidator()
    {
        RuleFor(x => x.Dto.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Apellido).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Email).EmailAddress().MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
        RuleFor(x => x.Dto.Telefono).MaximumLength(20);
        RuleFor(x => x.Dto.Direccion).MaximumLength(300);
    }
}
