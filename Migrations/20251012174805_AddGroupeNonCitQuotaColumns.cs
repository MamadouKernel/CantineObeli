using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obeli_K.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupeNonCitQuotaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeGroupe",
                table: "GroupesNonCit",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuotaJournalier",
                table: "GroupesNonCit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuotaNuit",
                table: "GroupesNonCit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RestrictionFormuleStandard",
                table: "GroupesNonCit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CodeVerification",
                table: "Commandes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DirectionId",
                table: "Commandes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfigurationsCommande",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valeur = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationsCommande", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Directions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExportCommandesPrestataire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomFichier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TailleFichier = table.Column<long>(type: "bigint", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateExport = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExportEffectue = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Commentaires = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportCommandesPrestataire", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportCommandesPrestataire_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrestataireCantines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Adresse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrestataireCantines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuotasJournaliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupeNonCitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuotaJour = table.Column<int>(type: "int", nullable: false),
                    QuotaNuit = table.Column<int>(type: "int", nullable: false),
                    PlatsConsommesJour = table.Column<int>(type: "int", nullable: false),
                    PlatsConsommesNuit = table.Column<int>(type: "int", nullable: false),
                    RestrictionFormuleStandard = table.Column<bool>(type: "bit", nullable: false),
                    Commentaires = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotasJournaliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotasJournaliers_GroupesNonCit_GroupeNonCitId",
                        column: x => x.GroupeNonCitId,
                        principalTable: "GroupesNonCit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_DirectionId",
                table: "Commandes",
                column: "DirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationsCommande_Cle",
                table: "ConfigurationsCommande",
                column: "Cle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExportCommandesPrestataire_DateDebut_DateFin",
                table: "ExportCommandesPrestataire",
                columns: new[] { "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_ExportCommandesPrestataire_DateExport",
                table: "ExportCommandesPrestataire",
                column: "DateExport");

            migrationBuilder.CreateIndex(
                name: "IX_ExportCommandesPrestataire_UtilisateurId",
                table: "ExportCommandesPrestataire",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_PrestataireCantines_Nom",
                table: "PrestataireCantines",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_QuotasJournaliers_GroupeNonCitId_Date",
                table: "QuotasJournaliers",
                columns: new[] { "GroupeNonCitId", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Commandes_Directions_DirectionId",
                table: "Commandes",
                column: "DirectionId",
                principalTable: "Directions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_Directions_DirectionId",
                table: "Commandes");

            migrationBuilder.DropTable(
                name: "ConfigurationsCommande");

            migrationBuilder.DropTable(
                name: "Directions");

            migrationBuilder.DropTable(
                name: "ExportCommandesPrestataire");

            migrationBuilder.DropTable(
                name: "PrestataireCantines");

            migrationBuilder.DropTable(
                name: "QuotasJournaliers");

            migrationBuilder.DropIndex(
                name: "IX_Commandes_DirectionId",
                table: "Commandes");

            migrationBuilder.DropColumn(
                name: "CodeGroupe",
                table: "GroupesNonCit");

            migrationBuilder.DropColumn(
                name: "QuotaJournalier",
                table: "GroupesNonCit");

            migrationBuilder.DropColumn(
                name: "QuotaNuit",
                table: "GroupesNonCit");

            migrationBuilder.DropColumn(
                name: "RestrictionFormuleStandard",
                table: "GroupesNonCit");

            migrationBuilder.DropColumn(
                name: "CodeVerification",
                table: "Commandes");

            migrationBuilder.DropColumn(
                name: "DirectionId",
                table: "Commandes");
        }
    }
}
