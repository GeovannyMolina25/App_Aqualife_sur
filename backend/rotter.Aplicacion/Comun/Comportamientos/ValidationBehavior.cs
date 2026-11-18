using FluentValidation;
using MediatR;

namespace rotter.Aplicacion.Comun.Comportamientos;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validadores;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validadores) => _validadores = validadores;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validadores.Any()) return await next();

        var contexto = new ValidationContext<TRequest>(request);
        var fallos = (await Task.WhenAll(_validadores.Select(v => v.ValidateAsync(contexto, ct))))
            .SelectMany(r => r.Errors)
            .ToList();

        if (fallos.Count != 0) throw new ValidationException(fallos);

        return await next();
    }
}
