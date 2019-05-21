using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalog.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    CatalogId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 400, nullable: false),
                    Sku = table.Column<string>(maxLength: 60, nullable: false),
                    PictureUri = table.Column<string>(maxLength: 450, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(9, 2)", nullable: false),
                    Description = table.Column<string>(maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.CatalogId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
