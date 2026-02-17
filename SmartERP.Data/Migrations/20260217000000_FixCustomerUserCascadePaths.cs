using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartERP.Data.Migrations
{
    /// <summary>
    /// Fixes SQL Server "multiple cascade paths" error by ensuring all Customer->User
    /// FKs use ON DELETE NO ACTION (required when a table has multiple FKs to the same parent).
    /// </summary>
    public class FixCustomerUserCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only apply for SQL Server (avoids errors on other providers)
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                // Only fix if Customers table exists (e.g. created by EnsureCreated with wrong FKs)
                migrationBuilder.Sql(@"
                    IF OBJECT_ID('Customers', 'U') IS NOT NULL
                    BEGIN
                        IF OBJECT_ID('FK_Customers_Users_LastModifiedBy', 'F') IS NOT NULL
                            ALTER TABLE [Customers] DROP CONSTRAINT [FK_Customers_Users_LastModifiedBy];
                        IF OBJECT_ID('FK_Customers_Users_CreatedBy', 'F') IS NOT NULL
                            ALTER TABLE [Customers] DROP CONSTRAINT [FK_Customers_Users_CreatedBy];

                        IF OBJECT_ID('FK_Customers_Users_CreatedBy', 'F') IS NULL
                            ALTER TABLE [Customers] ADD CONSTRAINT [FK_Customers_Users_CreatedBy]
                                FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
                        IF OBJECT_ID('FK_Customers_Users_LastModifiedBy', 'F') IS NULL
                            ALTER TABLE [Customers] ADD CONSTRAINT [FK_Customers_Users_LastModifiedBy]
                                FOREIGN KEY ([LastModifiedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
                    END
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(@"
                    IF OBJECT_ID('FK_Customers_Users_LastModifiedBy', 'F') IS NOT NULL
                        ALTER TABLE [Customers] DROP CONSTRAINT [FK_Customers_Users_LastModifiedBy];
                    IF OBJECT_ID('FK_Customers_Users_CreatedBy', 'F') IS NOT NULL
                        ALTER TABLE [Customers] DROP CONSTRAINT [FK_Customers_Users_CreatedBy];
                ");
            }
        }
    }
}
