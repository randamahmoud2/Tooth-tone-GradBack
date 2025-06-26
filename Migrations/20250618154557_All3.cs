using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class All3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Account",
                table: "AdminEarnings");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AdminEarnings");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AdminEarnings",
                newName: "Id");

            migrationBuilder.AlterColumn<double>(
                name: "TotalEarning",
                table: "DoctorsEarnings",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalEarningsFees",
                table: "AdminEarnings",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.CreateTable(
                name: "DoctorEarningDTO",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Account = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalEarning = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorEarningDTO", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorEarningDTO");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AdminEarnings",
                newName: "UserId");

            migrationBuilder.AlterColumn<float>(
                name: "TotalEarning",
                table: "DoctorsEarnings",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "TotalEarningsFees",
                table: "AdminEarnings",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Account",
                table: "AdminEarnings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AdminEarnings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
