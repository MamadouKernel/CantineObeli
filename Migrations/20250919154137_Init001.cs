using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obeli_K.Migrations
{
    /// <inheritdoc />
    public partial class Init001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_Commandes_CommandeIdCommande",
                table: "Utilisateurs");

            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_FormulesJour_FormuleJourIdFormule",
                table: "Utilisateurs");

            migrationBuilder.DropIndex(
                name: "IX_Utilisateurs_CommandeIdCommande",
                table: "Utilisateurs");

            migrationBuilder.DropIndex(
                name: "IX_Utilisateurs_FormuleJourIdFormule",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "CommandeIdCommande",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "FormuleJourIdFormule",
                table: "Utilisateurs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommandeIdCommande",
                table: "Utilisateurs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FormuleJourIdFormule",
                table: "Utilisateurs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_CommandeIdCommande",
                table: "Utilisateurs",
                column: "CommandeIdCommande");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_FormuleJourIdFormule",
                table: "Utilisateurs",
                column: "FormuleJourIdFormule");

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_Commandes_CommandeIdCommande",
                table: "Utilisateurs",
                column: "CommandeIdCommande",
                principalTable: "Commandes",
                principalColumn: "IdCommande");

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_FormulesJour_FormuleJourIdFormule",
                table: "Utilisateurs",
                column: "FormuleJourIdFormule",
                principalTable: "FormulesJour",
                principalColumn: "IdFormule");
        }
    }
}
