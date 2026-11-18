namespace rotter.Aplicacion.Productos.Commands;

/// <summary>Reglas de precio compartidas al crear y editar productos o servicios.</summary>
public static class ValidacionPrecios
{
    /// <returns>Mensaje de error, o null si los precios son válidos.</returns>
    public static string? Validar(decimal precio, bool esPromocion, decimal? precioPromocion)
    {
        if (precio < 0) return "El precio no puede ser negativo.";
        if (!esPromocion) return null;
        if (precioPromocion is null or <= 0) return "Ingresa el precio promocional.";
        if (precio <= 0) return "Para poner una promoción, primero define el precio normal.";
        if (precioPromocion >= precio) return "El precio promocional debe ser menor que el precio normal.";
        return null;
    }
}
