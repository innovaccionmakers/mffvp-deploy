using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contributions.Infrastructure.Database.Migrations.Contributions
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "contributions");

            migrationBuilder.CreateTable(
                name: "ClientOperations",
                schema: "contributions",
                columns: table => new
                {
                    ClientOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AffiliateId = table.Column<int>(type: "integer", nullable: false),
                    ObjectiveId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "integer", nullable: false),
                    SubTransactionTypeId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientOperations", x => x.ClientOperationId);
                });

            migrationBuilder.CreateTable(
                name: "Trusts",
                schema: "contributions",
                columns: table => new
                {
                    TrustId = table.Column<Guid>(type: "uuid", nullable: false),
                    AffiliateId = table.Column<int>(type: "integer", nullable: false),
                    ObjectiveId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    TotalBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalUnits = table.Column<decimal>(type: "numeric", nullable: true),
                    Principal = table.Column<decimal>(type: "numeric", nullable: false),
                    Earnings = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxCondition = table.Column<int>(type: "integer", nullable: false),
                    ContingentWithholding = table.Column<decimal>(type: "numeric", nullable: false),
                    EarningsWithholding = table.Column<decimal>(type: "numeric", nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trusts", x => x.TrustId);
                });

            migrationBuilder.CreateTable(
                name: "TrustOperations",
                schema: "contributions",
                columns: table => new
                {
                    TrustOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrustId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustOperations", x => x.TrustOperationId);
                    table.ForeignKey(
                        name: "FK_TrustOperations_ClientOperations_ClientOperationId",
                        column: x => x.ClientOperationId,
                        principalSchema: "contributions",
                        principalTable: "ClientOperations",
                        principalColumn: "ClientOperationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrustOperations_Trusts_TrustId",
                        column: x => x.TrustId,
                        principalSchema: "contributions",
                        principalTable: "Trusts",
                        principalColumn: "TrustId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrustOperations_ClientOperationId",
                schema: "contributions",
                table: "TrustOperations",
                column: "ClientOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustOperations_TrustId",
                schema: "contributions",
                table: "TrustOperations",
                column: "TrustId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustOperations",
                schema: "contributions");

            migrationBuilder.DropTable(
                name: "ClientOperations",
                schema: "contributions");

            migrationBuilder.DropTable(
                name: "Trusts",
                schema: "contributions");
        }
    }
}
