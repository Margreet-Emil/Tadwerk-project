using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RecyclingApp.Data;

#nullable disable

namespace RecyclingApp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261013180000_AddPolicyAndPenalty")]
    public partial class AddPolicyAndPenalty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollectorNote",
                table: "PickupRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PenaltyAmount",
                table: "PickupRequests",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyAccepted",
                table: "PickupRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyAcceptedAt",
                table: "PickupRequests",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CollectorNote", table: "PickupRequests");
            migrationBuilder.DropColumn(name: "PenaltyAmount", table: "PickupRequests");
            migrationBuilder.DropColumn(name: "PrivacyAccepted", table: "PickupRequests");
            migrationBuilder.DropColumn(name: "PrivacyAcceptedAt", table: "PickupRequests");
        }
    }
}
