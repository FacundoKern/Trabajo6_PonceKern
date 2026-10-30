namespace TiendaAPI.Helpers
{
    // En la base las fechas se guardan en UTC. Argentina es UTC-3 todo el año (sin horario de verano),
    // así que alcanza con un corrimiento fijo para mostrar y filtrar en hora local.
    public static class FechaHelper
    {
        private static readonly TimeSpan Diferencia = TimeSpan.FromHours(-3);
        //Resta 3 horas a una fecha UTC para obtener la hora de Argentina

        public static DateTime ALocal(DateTime fechaUtc) => fechaUtc + Diferencia;
        // InicioDiaUtc y FinDiaUtc hacen lo contrario conceptualmente: reciben un día de Argentina y
        // calculan sus límites expresados en UTC para filtrar correctamente la base de datos

        // 00:00 hora argentina del día indicado, expresado en UTC (para filtrar "desde")
        public static DateTime InicioDiaUtc(DateTime dia) => dia.Date - Diferencia;

        // 00:00 del día SIGUIENTE en UTC (para filtrar "hasta" con <, incluyendo todo el día)
        public static DateTime FinDiaUtc(DateTime dia) => dia.Date.AddDays(1) - Diferencia;
    }
}
