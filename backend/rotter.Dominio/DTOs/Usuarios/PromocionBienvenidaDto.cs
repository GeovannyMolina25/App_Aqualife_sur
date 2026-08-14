namespace rotter.Dominio.DTOs.Usuarios;

public static class PremiosBienvenida
{
    public const string SeptimoBotellonGratis = "SeptimoBotellonGratis";
    public const string BotellonLlavesGratis = "BotellonLlavesGratis";
    public const string TomaTodo = "TomaTodo";
}

/// <summary>
/// Los 8 sectores de la ruleta. "Sorpresa" y "7º Botellón Gratis" son solo etiquetas distintas
/// del mismo premio real <see cref="PremiosBienvenida.SeptimoBotellonGratis"/> — nunca se guarda
/// "Sorpresa" en la base de datos. El sorteo NO es uniforme por cantidad de casillas: cada sector
/// lleva un <c>Peso</c> explícito (de 100) que fija su probabilidad real —
/// Botellón + Llaves Gratis = 1/100, Toma Todo = 5/100, y el 94/100 restante se reparte entre los
/// 6 sectores que otorgan Séptimo Botellón Gratis, que es por lejos el premio más frecuente.
/// </summary>
public static class SegmentosRuleta
{
    public static readonly (string Etiqueta, string PremioReal, int Peso)[] Segmentos =
    [
        ("Sorpresa", PremiosBienvenida.SeptimoBotellonGratis, 18),
        ("7º Botellón Gratis", PremiosBienvenida.SeptimoBotellonGratis, 11),
        ("Sorpresa", PremiosBienvenida.SeptimoBotellonGratis, 18),
        ("Botellón + Llaves Gratis", PremiosBienvenida.BotellonLlavesGratis, 1),
        ("Sorpresa", PremiosBienvenida.SeptimoBotellonGratis, 18),
        ("7º Botellón Gratis", PremiosBienvenida.SeptimoBotellonGratis, 11),
        ("Sorpresa", PremiosBienvenida.SeptimoBotellonGratis, 18),
        ("Toma Todo", PremiosBienvenida.TomaTodo, 5),
    ];
}

public record EstadoPromocionDto(
    bool DebeGirar,
    string? PremioBienvenida,
    int RecargasParaSeptimo,
    bool PremioBienvenidaEntregado
);

public record ResultadoGiroDto(
    bool YaGiro,
    string PremioMostrado,
    string PremioReal,
    int SegmentoIndice
);
