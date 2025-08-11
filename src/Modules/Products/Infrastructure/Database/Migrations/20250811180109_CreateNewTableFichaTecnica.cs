using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Products.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewTableFichaTecnica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ficha_tecnica",
                schema: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portafolio_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    aportes = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    retiros = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    pyg_bruto = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    gastos = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    comision_dia = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    costo_dia = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    rendimientos_abonados = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    rendimiento_bruto_unidad = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    costo_unidad = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    valor_unidad = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    unidades = table.Column<decimal>(type: "numeric(38,16)", nullable: false),
                    valor_portafolio = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    participes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ficha_tecnica", x => x.id);
                    table.ForeignKey(
                        name: "fk_ficha_tecnica_portafolios_portafolio_id",
                        column: x => x.portafolio_id,
                        principalSchema: "productos",
                        principalTable: "portafolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ficha_tecnica_portafolio_id",
                schema: "productos",
                table: "ficha_tecnica",
                column: "portafolio_id");

            migrationBuilder.CreateIndex(
                name: "ix_ficha_tecnica_fecha",
                schema: "productos",
                table: "ficha_tecnica",
                column: "fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ficha_tecnica",
                schema: "productos");
        }
    }
}
