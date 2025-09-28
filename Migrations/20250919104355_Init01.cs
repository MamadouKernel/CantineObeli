using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obeli_K.Migrations
{
    /// <inheritdoc />
    public partial class Init01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fonctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fonctions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupesNonCit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupesNonCit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypesFormule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesFormule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormulesJour",
                columns: table => new
                {
                    IdFormule = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Entree = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Dessert = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Plat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Garniture = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlatStandard1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GarnitureStandard1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlatStandard2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GarnitureStandard2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Feculent = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Legumes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Marge = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NomFormule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TypeFormuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Verrouille = table.Column<bool>(type: "bit", nullable: false),
                    Historique = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulesJour", x => x.IdFormule);
                    table.ForeignKey(
                        name: "FK_FormulesJour_TypesFormule_TypeFormuleId",
                        column: x => x.TypeFormuleId,
                        principalTable: "TypesFormule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Commandes",
                columns: table => new
                {
                    IdCommande = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateConsommation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusCommande = table.Column<int>(type: "int", nullable: false),
                    CodeCommande = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PeriodeService = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdFormule = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeClient = table.Column<int>(type: "int", nullable: false),
                    GroupeNonCitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisiteurNom = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    VisiteurTelephone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Site = table.Column<int>(type: "int", nullable: true),
                    DateLivraisonPrevueUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateReceptionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceptionConfirmeeParUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceptionConfirmeeParNom = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Quantite = table.Column<int>(type: "int", nullable: false),
                    Instantanee = table.Column<bool>(type: "bit", nullable: false),
                    AnnuleeParPrestataire = table.Column<bool>(type: "bit", nullable: false),
                    MotifAnnulation = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commandes", x => x.IdCommande);
                    table.ForeignKey(
                        name: "FK_Commandes_FormulesJour_IdFormule",
                        column: x => x.IdFormule,
                        principalTable: "FormulesJour",
                        principalColumn: "IdFormule",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Commandes_GroupesNonCit_GroupeNonCitId",
                        column: x => x.GroupeNonCitId,
                        principalTable: "GroupesNonCit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenoms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Lieu = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CodeCommande = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MotDePasseHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodePinHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResetTokenHash = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: true),
                    ResetExpireLeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResetUtilise = table.Column<bool>(type: "bit", nullable: false),
                    MustResetPassword = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FonctionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Site = table.Column<int>(type: "int", nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CommandeIdCommande = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormuleJourIdFormule = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Commandes_CommandeIdCommande",
                        column: x => x.CommandeIdCommande,
                        principalTable: "Commandes",
                        principalColumn: "IdCommande");
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Departements_DepartementId",
                        column: x => x.DepartementId,
                        principalTable: "Departements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Fonctions_FonctionId",
                        column: x => x.FonctionId,
                        principalTable: "Fonctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_FormulesJour_FormuleJourIdFormule",
                        column: x => x.FormuleJourIdFormule,
                        principalTable: "FormulesJour",
                        principalColumn: "IdFormule");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_GroupeNonCitId",
                table: "Commandes",
                column: "GroupeNonCitId");

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_IdFormule",
                table: "Commandes",
                column: "IdFormule");

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_UtilisateurId",
                table: "Commandes",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulesJour_TypeFormuleId",
                table: "FormulesJour",
                column: "TypeFormuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_CommandeIdCommande",
                table: "Utilisateurs",
                column: "CommandeIdCommande");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_DepartementId",
                table: "Utilisateurs",
                column: "DepartementId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_FonctionId",
                table: "Utilisateurs",
                column: "FonctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_FormuleJourIdFormule",
                table: "Utilisateurs",
                column: "FormuleJourIdFormule");

            migrationBuilder.AddForeignKey(
                name: "FK_Commandes_Utilisateurs_UtilisateurId",
                table: "Commandes",
                column: "UtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_FormulesJour_IdFormule",
                table: "Commandes");

            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_FormulesJour_FormuleJourIdFormule",
                table: "Utilisateurs");

            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_GroupesNonCit_GroupeNonCitId",
                table: "Commandes");

            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_Utilisateurs_UtilisateurId",
                table: "Commandes");

            migrationBuilder.DropTable(
                name: "FormulesJour");

            migrationBuilder.DropTable(
                name: "TypesFormule");

            migrationBuilder.DropTable(
                name: "GroupesNonCit");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Commandes");

            migrationBuilder.DropTable(
                name: "Departements");

            migrationBuilder.DropTable(
                name: "Fonctions");
        }
    }
}
