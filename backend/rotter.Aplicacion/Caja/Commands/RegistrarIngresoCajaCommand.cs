using MediatR;
using rotter.Dominio.DTOs.Caja;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Caja.Commands;

public record RegistrarIngresoCajaCommand(CrearIngresoCajaDto Dto, int UsuarioId) : IRequest<RespuestaDto<IngresoCajaDto>>;

public class RegistrarIngresoCajaHandler : IRequestHandler<RegistrarIngresoCajaCommand, RespuestaDto<IngresoCajaDto>>
{
    private readonly ICajaRepositorio _caja;
    private readonly IAuditoriaServicio _auditoria;

    public RegistrarIngresoCajaHandler(ICajaRepositorio caja, IAuditoriaServicio auditoria)
    { _caja = caja; _auditoria = auditoria; }

    public async Task<RespuestaDto<IngresoCajaDto>> Handle(RegistrarIngresoCajaCommand req, CancellationToken ct)
    {
        if (req.Dto.TotalIngresado <= 0)
            return RespuestaDto<IngresoCajaDto>.Fallo("El total a ingresar debe ser mayor a cero.", 400);

        var numero = await _caja.GenerarNumeroIngresoAsync();
        var ingreso = new IngresoCaja
        {
            NumeroIngreso = numero,
            Banco = req.Dto.Banco,
            NumeroTransaccion = req.Dto.NumeroTransaccion,
            ComprobanteUrl = req.Dto.ComprobanteUrl,
            TotalIngresado = req.Dto.TotalIngresado,
            Observacion = req.Dto.Observacion,
            UsuarioId = req.UsuarioId,
            FechaIngreso = DateTime.Now,
        };

        await _caja.CrearIngresoAsync(ingreso, req.Dto.VentaIds ?? new List<int>());
        await _auditoria.RegistrarAsync("REGISTRAR_INGRESO_CAJA", "Caja", ingreso.Id.ToString(),
            datosNuevos: new { ingreso.NumeroIngreso, ingreso.TotalIngresado });

        var creado = await _caja.ObtenerIngresoPorIdAsync(ingreso.Id);
        return RespuestaDto<IngresoCajaDto>.Ok(Mapear(creado!), "Ingreso registrado exitosamente.");
    }

    public static IngresoCajaDto Mapear(IngresoCaja i) => new(
        i.Id, i.NumeroIngreso, i.Banco, i.NumeroTransaccion, i.ComprobanteUrl, i.TotalIngresado,
        i.Observacion, $"{i.Usuario.Nombre} {i.Usuario.Apellido}", i.FechaIngreso,
        i.Detalles.Select(d => d.VentaId).ToList());
}
