using MediatR;
using rotter.Dominio.Constantes;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Cotizaciones;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Cotizaciones.Commands;

public record SolicitarCotizacionCommand(CrearCotizacionServicioDto Dto, int? ClienteId) : IRequest<RespuestaDto<CotizacionServicioDto>>;

/// <summary>
/// Registra una solicitud de cotización de un servicio (lead, no una venta): no requiere sesión
/// (permite invitados para no ponerle fricción a la captación desde el landing/catálogo), pero
/// asocia el ClienteId si la petición llegó autenticada. Nunca falla la solicitud por un error de
/// correo — el aviso al negocio es best-effort.
/// </summary>
public class SolicitarCotizacionHandler : IRequestHandler<SolicitarCotizacionCommand, RespuestaDto<CotizacionServicioDto>>
{
    private readonly ICotizacionServicioRepositorio _cotizaciones;
    private readonly IProductoRepositorio _productos;
    private readonly IEmailServicio _email;

    public SolicitarCotizacionHandler(ICotizacionServicioRepositorio cotizaciones, IProductoRepositorio productos, IEmailServicio email)
    { _cotizaciones = cotizaciones; _productos = productos; _email = email; }

    public async Task<RespuestaDto<CotizacionServicioDto>> Handle(SolicitarCotizacionCommand req, CancellationToken ct)
    {
        var dto = req.Dto;
        if (string.IsNullOrWhiteSpace(dto.NombreContacto) || string.IsNullOrWhiteSpace(dto.Telefono) ||
            string.IsNullOrWhiteSpace(dto.Direccion) || string.IsNullOrWhiteSpace(dto.TamanoEspacio))
            return RespuestaDto<CotizacionServicioDto>.Fallo("Nombre, teléfono, dirección y tamaño del espacio son obligatorios.");

        var producto = await _productos.ObtenerPorIdAsync(dto.ProductoId);
        if (producto is null) return RespuestaDto<CotizacionServicioDto>.Fallo("Servicio no encontrado.", 404);

        var cotizacion = new CotizacionServicio
        {
            ProductoId = dto.ProductoId,
            ClienteId = req.ClienteId,
            NombreContacto = dto.NombreContacto.Trim(),
            Telefono = dto.Telefono.Trim(),
            Email = dto.Email,
            Direccion = dto.Direccion.Trim(),
            TamanoEspacio = dto.TamanoEspacio.Trim(),
            FechaDeseada = dto.FechaDeseada,
            Comentario = dto.Comentario,
        };

        await _cotizaciones.CrearAsync(cotizacion);
        await EnviarCorreoAsync(cotizacion, producto.Nombre);

        return RespuestaDto<CotizacionServicioDto>.Ok(
            Mapear(cotizacion, producto.Nombre),
            "¡Solicitud enviada! Te contactaremos pronto para coordinar tu cotización.");
    }

    public static CotizacionServicioDto Mapear(CotizacionServicio c, string servicioNombre) =>
        new(c.Id, c.ProductoId, servicioNombre, c.NombreContacto, c.Telefono, c.Email, c.Direccion,
            c.TamanoEspacio, c.FechaDeseada, c.Comentario, c.Estado, c.FechaCreacion);

    private async Task EnviarCorreoAsync(CotizacionServicio c, string servicioNombre)
    {
        try
        {
            var html = $@"
                <div style='font-family: Arial; padding: 20px; background: #f5f5f5;'>
                    <div style='max-width: 500px; margin: auto; background: white; border-radius: 12px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,.08);'>
                        <h2 style='color:#1a6b8a;'>Nueva solicitud de cotización</h2>
                        <p><strong>Servicio:</strong> {servicioNombre}</p>
                        <p><strong>Nombre:</strong> {c.NombreContacto}<br/>
                           <strong>Teléfono:</strong> {c.Telefono}<br/>
                           <strong>Email:</strong> {c.Email ?? "—"}<br/>
                           <strong>Dirección:</strong> {c.Direccion}<br/>
                           <strong>Tamaño del espacio:</strong> {c.TamanoEspacio}<br/>
                           <strong>Fecha deseada:</strong> {(c.FechaDeseada.HasValue ? c.FechaDeseada.Value.ToString("dd/MM/yyyy") : "—")}</p>
                        {(string.IsNullOrWhiteSpace(c.Comentario) ? "" : $"<p><strong>Comentario:</strong> {c.Comentario}</p>")}
                    </div>
                </div>";

            await _email.EnviarAsync(EmpresaConstantes.Correo, $"Nueva cotización — {servicioNombre}", html);

            if (!string.IsNullOrWhiteSpace(c.Email))
            {
                await _email.EnviarAsync(c.Email,
                    $"Recibimos tu solicitud de cotización — {EmpresaConstantes.Nombre}",
                    $@"
                    <div style='font-family: Arial; padding: 20px; background: #f5f5f5;'>
                        <div style='max-width: 500px; margin: auto; background: white; border-radius: 12px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,.08);'>
                            <h2 style='color:#1a6b8a;'>¡Gracias, {c.NombreContacto}!</h2>
                            <p>Recibimos tu solicitud de cotización para <strong>{servicioNombre}</strong>. Nos pondremos en contacto contigo al teléfono {c.Telefono} para coordinar los detalles.</p>
                        </div>
                    </div>");
            }
        }
        catch
        {
            // La solicitud ya quedó guardada; un fallo de correo no debe romper la respuesta al usuario.
        }
    }
}
