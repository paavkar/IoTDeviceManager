using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTDeviceManager.server.Migrations
{
    /// <inheritdoc />
    public partial class DevicesAndSensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    LastConnectionTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.UniqueConstraint("AK_Devices_SerialNumber", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_Devices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    LastConnectionTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MeasurementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LatestReading = table.Column<double>(type: "float", nullable: false),
                    DeviceSerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensors_Devices_DeviceSerialNumber",
                        column: x => x.DeviceSerialNumber,
                        principalTable: "Devices",
                        principalColumn: "SerialNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "DeviceSerialNumberIndex",
                table: "Devices",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_DeviceSerialNumber",
                table: "Sensors",
                column: "DeviceSerialNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices");
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Devices_DeviceSerialNumber",
                table: "Sensors");
            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
