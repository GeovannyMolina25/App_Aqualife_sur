using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Usuarios;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Usuarios.Commands;

public record GirarRuletaCommand(int UsuarioId) : IRequest<RespuestaDto<ResultadoGiroDto>>;

/// <summary>
/// Determina el premio de bienvenida en el backend (nunca en el cliente). Si el usuario
/// ya giró antes, no vuelve a sortear: devuelve el mismo resultado ya guardado, así el
/// frontend no puede forzar un segundo giro repitiendo la llamada.
/// </summary>
public class GirarRuletaHandler : IRequestHandler<GirarRuletaCommand, RespuestaDto<ResultadoGiroDto>>
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IAuditoriaServicio _auditoria;

    public GirarRuletaHandler(IUsuarioRepositorio usuarios, IAuditoriaServicio auditoria)
    { _usuarios = usuarios; _auditoria = auditoria; }

    public async Task<RespuestaDto<ResultadoGiroDto>> Handle(GirarRuletaCommand req, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (usuario is null) return RespuestaDto<ResultadoGiroDto>.Fallo("Usuario no encontrado.", 404);

        if (usuario.PremioBienvenida is not null)
        {
            var indiceExistente = Array.FindIndex(SegmentosRuleta.Segmentos, s => s.PremioReal == usuario.PremioBienvenida);
            var etiquetaExistente = SegmentosRuleta.Segmentos[indiceExistente].Etiqueta;
            return RespuestaDto<ResultadoGiroDto>.Ok(
                new ResultadoGiroDto(true, etiquetaExistente, usuario.PremioBienvenida, indiceExistente),
                "Ya habías girado la ruleta antes.");
        }

        var indice = SortearIndicePonderado();
        var (etiqueta, premioReal, _) = SegmentosRuleta.Segmentos[indice];

        usuario.PremioBienvenida = premioReal;
        usuario.FechaPremioBienvenida = DateTime.Now;
        await _usuarios.ActualizarAsync(usuario);

        await _auditoria.RegistrarAsync("GIRAR_RULETA_BIENVENIDA", "Usuarios", usuario.Id.ToString(),
            datosNuevos: new { premioReal, etiqueta });

        return RespuestaDto<ResultadoGiroDto>.Ok(
            new ResultadoGiroDto(false, etiqueta, premioReal, indice),
            "¡Felicidades por tu premio de bienvenida!");
    }

    /// <summary>
    /// Elige el sector ganador respetando el <c>Peso</c> (de 100) de cada uno, no la cantidad
    /// de sectores. Ej.: un sector con Peso=1 sale 1 de cada 100 giros en promedio.
    /// </summary>
    private static int SortearIndicePonderado()
    {
        var sorteo = Random.Shared.Next(1, 101);
        var acumulado = 0;
        for (var i = 0; i < SegmentosRuleta.Segmentos.Length; i++)
        {
            acumulado += SegmentosRuleta.Segmentos[i].Peso;
            if (sorteo <= acumulado) return i;
        }
        return SegmentosRuleta.Segmentos.Length - 1;
    }
}
