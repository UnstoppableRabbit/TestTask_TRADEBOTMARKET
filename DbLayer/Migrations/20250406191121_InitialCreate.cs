using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DbLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PairSpreads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FirstFutures = table.Column<int>(type: "integer", nullable: false),
                    SecondFutures = table.Column<int>(type: "integer", nullable: false),
                    FirstFuturesPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    SecondFuturesPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PairSpreads", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PairSpreads_Date_FirstFutures_SecondFutures",
                table: "PairSpreads",
                columns: new[] { "Date", "FirstFutures", "SecondFutures" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PairSpreads");
        }
    }
}
