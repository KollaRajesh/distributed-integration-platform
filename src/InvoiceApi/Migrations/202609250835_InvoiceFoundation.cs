using InvoiceApi.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace InvoiceApi.Migrations;

[DbContext(typeof(InvoiceDbContext))]
[Migration("202609250835_InvoiceFoundation")]
public partial class InvoiceFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Invoices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AmountDue = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                AmountPaid = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                Currency = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Invoices", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_TenantId",
            table: "Invoices",
            column: "TenantId");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "Invoices");
}
