namespace TiendaAPI.Helpers
{
    // En la base las fechas se guardan en UTC. Argentina es UTC-3 todo el año (sin horario de verano),
    // así que alcanza con un corrimiento fijo para mostrar y filtrar en hora local.
    public static class FechaHelper
    {
        private static readonly TimeSpan Diferencia = TimeSpan.FromHours(-3);

        public static DateTime ALocal(DateTime fechaUtc) => fechaUtc + Diferencia;

        // 00:00 hora argentina del día indicado, expresado en UTC (para filtrar "desde")
        public static DateTime InicioDiaUtc(DateTime dia) => dia.Date - Diferencia;

        // 00:00 del día SIGUIENTE en UTC (para filtrar "hasta" con <, incluyendo todo el día)
        public static DateTime FinDiaUtc(DateTime dia) => dia.Date.AddDays(1) - Diferencia;
    }
}
