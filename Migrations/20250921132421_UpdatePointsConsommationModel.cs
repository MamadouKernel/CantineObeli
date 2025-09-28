using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obeli_K.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePointsConsommationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Montant",
                table: "PointsConsommation");

            migrationBuilder.RenameColumn(
                name: "TypeConsommation",
                table: "PointsConsommation",
                newName: "TypeFormule");

            migrationBuilder.RenameColumn(
                name: "Quantite",
                table: "PointsConsommation",
                newName: "QuantiteConsommee");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "PointsConsommation",
                newName: "NomPlat");

            migrationBuilder.AddColumn<Guid>(
                name: "CommandeId",
                table: "PointsConsommation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PointsConsommation_CommandeId",
                table: "PointsConsommation",
                column: "CommandeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PointsConsommation_Commandes_CommandeId",
                table: "PointsConsommation",
                column: "CommandeId",
                principalTable: "Commandes",
                principalColumn: "IdCommande",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointsConsommation_Commandes_CommandeId",
                table: "PointsConsommation");

            migrationBuilder.DropIndex(
                name: "IX_PointsConsommation_CommandeId",
                table: "PointsConsommation");

            migrationBuilder.DropColumn(
                name: "CommandeId",
                table: "PointsConsommation");

            migrationBuilder.RenameColumn(
                name: "TypeFormule",
                table: "PointsConsommation",
                newName: "TypeConsommation");

            migrationBuilder.RenameColumn(
                name: "QuantiteConsommee",
                table: "PointsConsommation",
                newName: "Quantite");

            migrationBuilder.RenameColumn(
                name: "NomPlat",
                table: "PointsConsommation",
                newName: "Description");

            migrationBuilder.AddColumn<decimal>(
                name: "Montant",
                table: "PointsConsommation",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
