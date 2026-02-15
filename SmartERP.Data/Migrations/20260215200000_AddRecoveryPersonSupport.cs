using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecoveryPersonSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create RecoveryPersons table
            migrationBuilder.CreateTable(
                name: "RecoveryPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecoveryPersons", x => x.Id);
                });

            // Step 2: Add RecoveryPersonId column to Billings (if it doesn't exist)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Billings' AND COLUMN_NAME = 'RecoveryPersonId'
                )
                BEGIN
                    ALTER TABLE [Billings] ADD [RecoveryPersonId] [int] NULL
                END
            ");

            // Step 3: Create index on RecoveryPersonId
            migrationBuilder.CreateIndex(
                name: "IX_Billings_RecoveryPersonId",
                table: "Billings",
                column: "RecoveryPersonId");

            // Step 4: Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Billings_RecoveryPersons_RecoveryPersonId",
                table: "Billings",
                column: "RecoveryPersonId",
                principalTable: "RecoveryPersons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_RecoveryPersons_RecoveryPersonId",
                table: "Billings");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_Billings_RecoveryPersonId",
                table: "Billings");

            // Drop RecoveryPersons table
            migrationBuilder.DropTable(
                name: "RecoveryPersons");

            // Drop RecoveryPersonId column from Billings (if it exists)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Billings' AND COLUMN_NAME = 'RecoveryPersonId'
                )
                BEGIN
                    ALTER TABLE [Billings] DROP CONSTRAINT [FK_Billings_RecoveryPersons_RecoveryPersonId]
                    ALTER TABLE [Billings] DROP COLUMN [RecoveryPersonId]
                END
            ");
        }
    }
}
