using System.Globalization;

namespace TiendaApp.Services;

public static class Formato
{
    private static readonly CultureInfo Argentina = new("es-AR");

    // 9200 -> "$ 9.200,00"
    public static string Precio(decimal valor) => valor.ToString("C2", Argentina);
}
