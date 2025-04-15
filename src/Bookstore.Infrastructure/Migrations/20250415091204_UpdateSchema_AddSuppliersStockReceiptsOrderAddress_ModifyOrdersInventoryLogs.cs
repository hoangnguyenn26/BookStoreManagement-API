using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema_AddSuppliersStockReceiptsOrderAddress_ModifyOrdersInventoryLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Country",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_PhoneNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_PostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_RecipientName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Street",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_State",
                table: "Orders",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Addresses",
                newName: "Village");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "Addresses",
                newName: "District");

            migrationBuilder.AddColumn<byte>(
                name: "DeliveryMethod",
                table: "Orders",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderShippingAddressId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "OrderType",
                table: "Orders",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PaymentMethod",
                table: "Orders",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<Guid>(
                name: "StockReceiptId",
                table: "InventoryLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderShippingAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Village = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderShippingAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockReceipts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StockReceiptDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityReceived = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReceiptDetails", x => x.Id);
                    table.CheckConstraint("CK_StockReceiptDetails_QuantityReceived", "[QuantityReceived] > 0");
                    table.ForeignKey(
                        name: "FK_StockReceiptDetails_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockReceiptDetails_StockReceipts_StockReceiptId",
                        column: x => x.StockReceiptId,
                        principalTable: "StockReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_InvoiceNumber",
                table: "Orders",
                column: "InvoiceNumber",
                unique: true,
                filter: "[InvoiceNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderShippingAddressId",
                table: "Orders",
                column: "OrderShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLogs_StockReceiptId",
                table: "InventoryLogs",
                column: "StockReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReceiptDetails_BookId",
                table: "StockReceiptDetails",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReceiptDetails_StockReceiptId",
                table: "StockReceiptDetails",
                column: "StockReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReceipts_SupplierId",
                table: "StockReceipts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Email",
                table: "Suppliers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryLogs_StockReceipts_StockReceiptId",
                table: "InventoryLogs",
                column: "StockReceiptId",
                principalTable: "StockReceipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderShippingAddresses_OrderShippingAddressId",
                table: "Orders",
                column: "OrderShippingAddressId",
                principalTable: "OrderShippingAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryLogs_StockReceipts_StockReceiptId",
                table: "InventoryLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderShippingAddresses_OrderShippingAddressId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderShippingAddresses");

            migrationBuilder.DropTable(
                name: "StockReceiptDetails");

            migrationBuilder.DropTable(
                name: "StockReceipts");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Orders_InvoiceNumber",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderShippingAddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_InventoryLogs_StockReceiptId",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderShippingAddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StockReceiptId",
                table: "InventoryLogs");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Orders",
                newName: "ShippingAddress_State");

            migrationBuilder.RenameColumn(
                name: "Village",
                table: "Addresses",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "Addresses",
                newName: "PostalCode");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Orders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_City",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Country",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_PhoneNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_PostalCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_RecipientName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_Street",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Books",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
