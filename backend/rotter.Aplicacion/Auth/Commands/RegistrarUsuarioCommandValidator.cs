using FluentValidation;

namespace rotter.Aplicacion.Auth.Commands;

public class RegistrarUsuarioCommandValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioCommandValidator()
    {
        RuleFor(x => x.Dto.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Apellido).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Dto.Password)
            .NotEmpty()
            .MinimumLength(10)
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un dígito.")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un símbolo.");
        RuleFor(x => x.Dto.FechaNacimiento)
            .LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("La fecha de nacimiento no puede ser futura.")
            .GreaterThan(new DateOnly(1900, 1, 1));
        RuleFor(x => x.Dto.Sexo).NotEmpty();
        RuleFor(x => x.Dto.Direccion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Dto.Telefono).MaximumLength(20);
    }
}
