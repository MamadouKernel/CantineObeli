using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obeli_K.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_Directions_DirectionId",
                table: "Commandes");

            migrationBuilder.DropTable(
                name: "Directions");

            migrationBuilder.DropIndex(
                name: "IX_Commandes_DirectionId",
                table: "Commandes");

            migrationBuilder.DropColumn(
                name: "DirectionId",
                table: "Commandes");

            migrationBuilder.AddColumn<int>(
                name: "MargeJourRestante",
                table: "FormulesJour",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MargeNuitRestante",
                table: "FormulesJour",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuotaJourRestant",
                table: "FormulesJour",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuotaNuitRestant",
                table: "FormulesJour",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MargeJourRestante",
                table: "FormulesJour");

            migrationBuilder.DropColumn(
                name: "MargeNuitRestante",
                table: "FormulesJour");

            migrationBuilder.DropColumn(
                name: "QuotaJourRestant",
                table: "FormulesJour");

            migrationBuilder.DropColumn(
                name: "QuotaNuitRestant",
                table: "FormulesJour");

            migrationBuilder.AddColumn<Guid>(
                name: "DirectionId",
                table: "Commandes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Directions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_DirectionId",
                table: "Commandes",
                column: "DirectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commandes_Directions_DirectionId",
                table: "Commandes",
                column: "DirectionId",
                principalTable: "Directions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
