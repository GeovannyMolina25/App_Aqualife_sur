using MediatR;
using rotter.Dominio.DTOs.Caja;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Caja.Commands;

public record RegistrarSalidaCajaCommand(CrearSalidaCajaDto Dto, int UsuarioId) : IRequest<RespuestaDto<SalidaCajaDto>>;

public class RegistrarSalidaCajaHandler : IRequestHandler<RegistrarSalidaCajaCommand, RespuestaDto<SalidaCajaDto>>
{
    private readonly ICajaRepositorio _caja;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IAuditoriaServicio _auditoria;

    public RegistrarSalidaCajaHandler(ICajaRepositorio caja, IUsuarioRepositorio usuarios, IAuditoriaServicio auditoria)
    { _caja = caja; _usuarios = usuarios; _auditoria = auditoria; }

    public async Task<RespuestaDto<SalidaCajaDto>> Handle(RegistrarSalidaCajaCommand req, CancellationToken ct)
    {
        if (req.Dto.Valor <= 0)
            return RespuestaDto<SalidaCajaDto>.Fallo("El valor a retirar debe ser mayor a cero.", 400);

        var saldo = await _caja.ObtenerSaldoAsync();
        if (req.Dto.Valor > saldo.Saldo)
            return RespuestaDto<SalidaCajaDto>.Fallo($"Saldo insuficiente en caja. Disponible: {saldo.Saldo:0.00}.", 400);

        var numero = await _caja.GenerarNumeroSalidaAsync();
        var salida = new SalidaCaja
        {
            NumeroSalida = numero,
            Valor = req.Dto.Valor,
            Banco = req.Dto.Banco,
            NumeroTransaccion = req.Dto.NumeroTransaccion,
            ComprobanteUrl = req.Dto.ComprobanteUrl,
            Motivo = req.Dto.Motivo,
            Descripcion = req.Dto.Descripcion,
            UsuarioId = req.UsuarioId,
            FechaSalida = DateTime.Now,
        };

        await _caja.CrearSalidaAsync(salida);
        await _auditoria.RegistrarAsync("REGISTRAR_SALIDA_CAJA", "Caja", salida.Id.ToString(),
            datosNuevos: new { salida.NumeroSalida, salida.Valor, salida.Motivo });

        var usuario = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        return RespuestaDto<SalidaCajaDto>.Ok(new SalidaCajaDto(
            salida.Id, salida.NumeroSalida, salida.FechaSalida, salida.Valor, salida.Banco,
            salida.NumeroTransaccion, salida.ComprobanteUrl, salida.Motivo, salida.Descripcion,
            $"{usuario!.Nombre} {usuario.Apellido}"), "Salida registrada exitosamente.");
    }
}
