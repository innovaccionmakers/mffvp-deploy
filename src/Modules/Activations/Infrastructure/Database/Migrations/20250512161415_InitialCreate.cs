using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Activations.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "activations");

            migrationBuilder.CreateTable(
                name: "affiliates",
                schema: "activations",
                columns: table => new
                {
                    affiliate_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    identification_type = table.Column<string>(type: "text", nullable: false),
                    identification = table.Column<string>(type: "text", nullable: false),
                    pensioner = table.Column<bool>(type: "boolean", nullable: false),
                    meets_requirements = table.Column<bool>(type: "boolean", nullable: false),
                    activation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_affiliates", x => x.affiliate_id);
                });

            migrationBuilder.CreateTable(
                name: "meets_pension_requirements",
                schema: "activations",
                columns: table => new
                {
                    meets_pension_requirement_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    affiliate_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meets_pension_requirements", x => x.meets_pension_requirement_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "affiliates",
                schema: "activations");

            migrationBuilder.DropTable(
                name: "meets_pension_requirements",
                schema: "activations");
        }
    }
}
