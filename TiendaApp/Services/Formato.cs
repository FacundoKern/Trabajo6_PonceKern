using System.Globalization;

namespace TiendaApp.Services;

public static class Formato
{
    private static readonly CultureInfo Argentina = new("es-AR");

    // 9200 -> "$ 9.200,00"
    public static string Precio(decimal valor) => valor.ToString("C2", Argentina);

    // La API guarda las fechas en UTC; se muestran en la hora del teléfono
    public static string FechaHora(DateTime fechaUtc) =>
        DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm", Argentina);
}
