using Microsoft.EntityFrameworkCore.Migrations;

namespace Store.Infrastructure.Migrations
{
    public partial class v2_Address : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Orders_OrderId",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem");

            migrationBuilder.RenameTable(
                name: "OrderItem",
                newName: "OrderItems");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_State",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Address_State",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Address_ZipCode",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItem");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItem",
                newName: "IX_OrderItem_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Orders_OrderId",
                table: "OrderItem",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
