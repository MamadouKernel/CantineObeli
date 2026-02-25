using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obeli_K.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDepartementEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Departements_DepartementId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_Departements_DepartementId",
                table: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Departements");

            migrationBuilder.DropIndex(
                name: "IX_Utilisateurs_DepartementId",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "DepartementId",
                table: "Utilisateurs");

            migrationBuilder.RenameColumn(
                name: "DepartementId",
                table: "Services",
                newName: "DirectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Services_DepartementId",
                table: "Services",
                newName: "IX_Services_DirectionId");

            migrationBuilder.AddColumn<Guid>(
                name: "DirectionId",
                table: "Utilisateurs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_DirectionId",
                table: "Utilisateurs",
                column: "DirectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Directions_DirectionId",
                table: "Services",
                column: "DirectionId",
                principalTable: "Directions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_Directions_DirectionId",
                table: "Utilisateurs",
                column: "DirectionId",
                principalTable: "Directions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Directions_DirectionId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_Directions_DirectionId",
                table: "Utilisateurs");

            migrationBuilder.DropIndex(
                name: "IX_Utilisateurs_DirectionId",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "DirectionId",
                table: "Utilisateurs");

            migrationBuilder.RenameColumn(
                name: "DirectionId",
                table: "Services",
                newName: "DepartementId");

            migrationBuilder.RenameIndex(
                name: "IX_Services_DirectionId",
                table: "Services",
                newName: "IX_Services_DepartementId");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartementId",
                table: "Utilisateurs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Departements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supprimer = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departements_Directions_DirectionId",
                        column: x => x.DirectionId,
                        principalTable: "Directions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_DepartementId",
                table: "Utilisateurs",
                column: "DepartementId");

            migrationBuilder.CreateIndex(
                name: "IX_Departements_DirectionId",
                table: "Departements",
                column: "DirectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Departements_DepartementId",
                table: "Services",
                column: "DepartementId",
                principalTable: "Departements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_Departements_DepartementId",
                table: "Utilisateurs",
                column: "DepartementId",
                principalTable: "Departements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
