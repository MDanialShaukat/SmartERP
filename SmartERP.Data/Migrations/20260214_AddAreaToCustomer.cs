using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Areas table first
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    LastModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Areas_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Areas_Users_LastModifiedBy",
                        column: x => x.LastModifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            // Create index on AreaName for uniqueness
            migrationBuilder.CreateIndex(
                name: "IX_Areas_AreaName",
                table: "Areas",
                column: "AreaName",
                unique: true);

            // Create indexes for foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_Areas_CreatedBy",
                table: "Areas",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_LastModifiedBy",
                table: "Areas",
                column: "LastModifiedBy");

            // Add AreaId column to Customers table
            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 1); // Default to first area (will be updated in data migration if needed)

            // Insert default areas if they don't exist
            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "AreaName", "Description", "IsActive", "CreatedDate", "CreatedBy" },
                values: new object[,]
                {
                    { "Model town", "Model town Area", true, DateTime.Now, null },
                    { "Ayoubia town", "Ayoubia town Area", true, DateTime.Now, null },
                    { "Mohra shah wali shah", "Mohra shah wali shah Area", true, DateTime.Now, null },
                    { "Shahbaz town", "Shahbaz town Area", true, DateTime.Now, null },
                    { "Canal town", "Canal Town Area", true, DateTime.Now, null },
                    { "HMC", "HMC Area", true, DateTime.Now, null },
                    { "Faisal town", "Faisal town Area", true, DateTime.Now, null }
                });

            // Add foreign key constraint for AreaId
            migrationBuilder.CreateIndex(
                name: "IX_Customers_AreaId",
                table: "Customers",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Areas_AreaId",
                table: "Customers",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Drop the old City column if it exists
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Customers' AND COLUMN_NAME='City') ALTER TABLE Customers DROP COLUMN City");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Areas_AreaId",
                table: "Customers");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_Customers_AreaId",
                table: "Customers");

            // Remove AreaId column from Customers
            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Customers");

            // Drop Areas table
            migrationBuilder.DropTable(
                name: "Areas");
        }
    }
}
