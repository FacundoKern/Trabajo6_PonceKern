namespace TiendaAPI.Helpers
{
    // Simula una pasarela de pago (no se cobra nada de verdad).
    // Reglas para la demo:
    //  - Efectivo y Transferencia: siempre aprobados.
    //  - Tarjeta: el número debe tener entre 13 y 19 dígitos. Si termina en 0000 se RECHAZA,
    //    así se puede mostrar en la presentación qué pasa con un pago rechazado.
    public static class PagoSimulado
    {
        public static readonly string[] MediosPermitidos = { "Efectivo", "Tarjeta", "Transferencia" };

        public record Resultado(bool Aprobado, string Mensaje, string? CodigoOperacion, string? TarjetaUltimos4);

        public static Resultado Procesar(string medioPago, string? numeroTarjeta)
        {
            if (!MediosPermitidos.Contains(medioPago))
                return new(false, $"Medio de pago inválido. Use: {string.Join(", ", MediosPermitidos)}", null, null);

            string? ultimos4 = null;

            if (medioPago == "Tarjeta")
            {
                var digitos = new string((numeroTarjeta ?? "").Where(char.IsDigit).ToArray());
                if (digitos.Length < 13 || digitos.Length > 19)
                    return new(false, "Número de tarjeta inválido", null, null);

                ultimos4 = digitos[^4..];
                if (ultimos4 == "0000")
                    return new(false, "Pago rechazado por la entidad emisora (simulado)", null, ultimos4);
            }

            var codigo = $"OP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            return new(true, "Pago aprobado", codigo, ultimos4);
        }
    }
}
