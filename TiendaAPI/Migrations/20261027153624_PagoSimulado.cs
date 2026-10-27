using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaAPI.Migrations
{
    /// <inheritdoc />
    public partial class PagoSimulado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las ventas que ya existían quedan como pagadas en efectivo y aprobadas
            migrationBuilder.AddColumn<string>(
                name: "CodigoOperacion",
                table: "Salidas",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoPago",
                table: "Salidas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Aprobado");

            migrationBuilder.AddColumn<string>(
                name: "MedioPago",
                table: "Salidas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Efectivo");

            migrationBuilder.AddColumn<string>(
                name: "TarjetaUltimos4",
                table: "Salidas",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoOperacion",
                table: "Salidas");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "Salidas");

            migrationBuilder.DropColumn(
                name: "MedioPago",
                table: "Salidas");

            migrationBuilder.DropColumn(
                name: "TarjetaUltimos4",
                table: "Salidas");
        }
    }
}
