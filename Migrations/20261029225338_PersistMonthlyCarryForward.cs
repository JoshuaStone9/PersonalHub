using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagerAspNet.Migrations
{
    public partial class PersistMonthlyCarryForward : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID('dbo.monthly_carry_forward', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.monthly_carry_forward(
                        monthly_carry_forward_id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_monthly_carry_forward PRIMARY KEY,
                        [year] int NOT NULL,
                        [month] int NOT NULL,
                        amount decimal(18,2) NOT NULL CONSTRAINT DF_monthly_carry_forward_amount DEFAULT 0,
                        override_amount decimal(18,2) NULL,
                        override_reason nvarchar(500) NULL,
                        source_year int NULL,
                        source_month int NULL,
                        updated_at datetime2 NOT NULL CONSTRAINT DF_monthly_carry_forward_updated_at DEFAULT SYSUTCDATETIME()
                    );

                    EXEC sys.sp_addextendedproperty
                        @name = N'FinanceManagerMigrationOwner', @value = N'PersistMonthlyCarryForward',
                        @level0type = N'SCHEMA', @level0name = N'dbo',
                        @level1type = N'TABLE', @level1name = N'monthly_carry_forward';
                END;

                IF COL_LENGTH('dbo.monthly_carry_forward', 'override_amount') IS NULL
                    ALTER TABLE dbo.monthly_carry_forward ADD override_amount decimal(18,2) NULL;

                IF COL_LENGTH('dbo.monthly_carry_forward', 'override_reason') IS NULL
                    ALTER TABLE dbo.monthly_carry_forward ADD override_reason nvarchar(500) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.monthly_carry_forward') AND [name] = 'UQ_monthly_carry_forward')
                    CREATE UNIQUE INDEX UQ_monthly_carry_forward ON dbo.monthly_carry_forward([year], [month]);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.extended_properties ep
                    INNER JOIN sys.tables t ON t.object_id = ep.major_id
                    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
                    WHERE s.[name] = 'dbo'
                      AND t.[name] = 'monthly_carry_forward'
                      AND ep.[name] = 'FinanceManagerMigrationOwner'
                      AND CONVERT(nvarchar(128), ep.[value]) = 'PersistMonthlyCarryForward')
                    DROP TABLE dbo.monthly_carry_forward;
                """);
        }
    }
}
