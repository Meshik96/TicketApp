using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Venue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    GridX = table.Column<int>(type: "int", nullable: false),
                    GridY = table.Column<int>(type: "int", nullable: false),
                    Orientation = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sectors_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<int>(type: "int", nullable: false),
                    RowIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seats_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "EventDate", "Name", "Status", "Venue" },
                values: new object[] { 1, new DateTime(2026, 11, 15, 21, 0, 0, 0, DateTimeKind.Unspecified), "Concierto de Rock UNAJ", "Active", "Estadio Municipal de Berazategui" });

            migrationBuilder.InsertData(
                table: "Sectors",
                columns: new[] { "Id", "Capacity", "EventId", "GridX", "GridY", "Name", "Orientation", "Price" },
                values: new object[,]
                {
                    { 1, 50, 1, 0, 0, "VIP", "Horizontal", 150.00m },
                    { 2, 50, 1, 0, 1, "General", "Horizontal", 75.00m }
                });

            migrationBuilder.InsertData(
                table: "Seats",
                columns: new[] { "Id", "RowIdentifier", "SeatNumber", "SectorId", "Status", "Version" },
                values: new object[,]
                {
                    { new Guid("000c788e-b476-427c-af8c-404ffef4631b"), "B", 8, 2, "Available", 1 },
                    { new Guid("0046ad3c-2102-47a9-95fe-49f51a5d16b8"), "B", 10, 2, "Available", 1 },
                    { new Guid("048a7758-8cbe-46ac-8de6-91336071b1bf"), "A", 23, 1, "Available", 1 },
                    { new Guid("063fae1b-4c34-459d-8898-397bd40bba54"), "A", 11, 1, "Available", 1 },
                    { new Guid("0b6d5c0b-a2e6-40ab-90c3-036a16fda638"), "B", 49, 2, "Available", 1 },
                    { new Guid("0d34f331-f019-4f13-ba1c-f0e3d45f3ac1"), "B", 19, 2, "Available", 1 },
                    { new Guid("10f4c06a-3e16-442c-a8cd-473078de2c2d"), "A", 43, 1, "Available", 1 },
                    { new Guid("121a71e2-ba08-41be-8ee2-e2972248139e"), "B", 45, 2, "Available", 1 },
                    { new Guid("15af964b-6862-4778-9082-8177e4d07381"), "B", 7, 2, "Available", 1 },
                    { new Guid("1677fac8-372d-40be-b01e-8a3fd1cf7eda"), "A", 13, 1, "Available", 1 },
                    { new Guid("1a02d715-e85b-4cc7-95a1-61f1199041ab"), "B", 18, 2, "Available", 1 },
                    { new Guid("1d15850d-4ee0-437e-be70-cd6148c2da2b"), "B", 43, 2, "Available", 1 },
                    { new Guid("218d741f-90e0-4514-934f-83a08c094ebb"), "A", 1, 1, "Available", 1 },
                    { new Guid("23ccbc3f-a7e4-4cba-ad2b-297e7735d43a"), "A", 14, 1, "Available", 1 },
                    { new Guid("23f87182-c2a0-459a-a70f-e3bd461906f4"), "B", 34, 2, "Available", 1 },
                    { new Guid("243c6ee6-2d87-4342-a25b-5816019dcd16"), "B", 3, 2, "Available", 1 },
                    { new Guid("2599bf56-1cfa-40df-a677-32d3b7623362"), "A", 27, 1, "Available", 1 },
                    { new Guid("285b40d5-e291-4d63-b0f1-42d0b02c330c"), "B", 42, 2, "Available", 1 },
                    { new Guid("29bd1d16-7a07-49c7-a22f-88f4b513c9e8"), "B", 26, 2, "Available", 1 },
                    { new Guid("29f3220e-6992-49c9-8357-7086881f0b6d"), "A", 49, 1, "Available", 1 },
                    { new Guid("2a90ef91-4d6b-4207-9410-4f91e69a35aa"), "A", 44, 1, "Available", 1 },
                    { new Guid("2e8e933a-4b42-4c8a-bc52-d759274716c4"), "A", 45, 1, "Available", 1 },
                    { new Guid("3012f4d1-4105-44c0-a197-df7e7cd02713"), "B", 31, 2, "Available", 1 },
                    { new Guid("38285b2a-d56a-4fad-a4d9-53f03fa8b3aa"), "A", 28, 1, "Available", 1 },
                    { new Guid("392a864a-3fda-4e01-8bca-80dd91f2324d"), "A", 15, 1, "Available", 1 },
                    { new Guid("39580930-4108-44eb-acbe-7a65b09aa6c6"), "A", 22, 1, "Available", 1 },
                    { new Guid("3c9d2f36-19e9-4435-8e93-2f3cc1910ec0"), "A", 37, 1, "Available", 1 },
                    { new Guid("3e14ac73-6f1b-4032-84a5-d1c1ea27ae47"), "A", 24, 1, "Available", 1 },
                    { new Guid("40e386e7-9e45-44c8-a490-25aa2bcebcc0"), "A", 29, 1, "Available", 1 },
                    { new Guid("44b9a8b6-49ac-4faf-9feb-a9a12703c10b"), "B", 23, 2, "Available", 1 },
                    { new Guid("44d4e2a6-4811-47be-aefd-40cdf0bf82b2"), "A", 48, 1, "Available", 1 },
                    { new Guid("4917b485-e424-49e3-97b1-3292bb80d10d"), "A", 9, 1, "Available", 1 },
                    { new Guid("4bfd0f98-b9be-4d72-ae81-ac6c6b5af1a2"), "A", 20, 1, "Available", 1 },
                    { new Guid("4cf10c33-64e0-4c5f-aba7-0c5e9be76ef2"), "B", 17, 2, "Available", 1 },
                    { new Guid("4e354db3-c7b3-4cc5-b749-85d4edc85300"), "B", 30, 2, "Available", 1 },
                    { new Guid("51e4365a-c524-4d19-97aa-8870552a913f"), "B", 44, 2, "Available", 1 },
                    { new Guid("5345358d-c1f1-4cc8-a1d8-949e94f60c4e"), "B", 1, 2, "Available", 1 },
                    { new Guid("540d25f3-e523-4f84-b381-76e73df42ffc"), "B", 22, 2, "Available", 1 },
                    { new Guid("55eba692-a844-4bba-be34-084092f512d4"), "B", 29, 2, "Available", 1 },
                    { new Guid("5a295369-9f1f-4f63-ad99-8e4c9b411897"), "A", 25, 1, "Available", 1 },
                    { new Guid("5a649a0b-d008-4715-b5c0-f45e3a67f79a"), "B", 6, 2, "Available", 1 },
                    { new Guid("5b3d307c-2888-4a2f-a40e-a376bf18bdd4"), "A", 38, 1, "Available", 1 },
                    { new Guid("5de8f77a-6b9a-4cce-a061-eef0a7546fee"), "A", 36, 1, "Available", 1 },
                    { new Guid("666cde4c-f32c-430b-88a8-55ac06b17700"), "A", 34, 1, "Available", 1 },
                    { new Guid("6907acde-f57d-4abf-b56f-c0a2915284d4"), "B", 9, 2, "Available", 1 },
                    { new Guid("6e627179-45ed-40f9-9ee1-f7699caff218"), "B", 47, 2, "Available", 1 },
                    { new Guid("761e5233-4276-4582-8a12-b9253d422d17"), "B", 13, 2, "Available", 1 },
                    { new Guid("79dfff3b-30b0-48e8-9955-bebecad38a44"), "B", 41, 2, "Available", 1 },
                    { new Guid("7a442203-9653-4027-aa29-afa41a14c92f"), "A", 12, 1, "Available", 1 },
                    { new Guid("7a8a7ee8-5e22-4abf-8ddf-9a28af97d25e"), "A", 31, 1, "Available", 1 },
                    { new Guid("7d1f94da-e011-488e-b542-802abc5776b6"), "B", 4, 2, "Available", 1 },
                    { new Guid("7e494dfc-3fa3-4ed2-bfd0-a9cfd68268a9"), "A", 40, 1, "Available", 1 },
                    { new Guid("80785fb8-5003-4400-bfb9-bb27c5765b70"), "A", 10, 1, "Available", 1 },
                    { new Guid("85f03210-70cb-4382-98fa-049ab4c8b73d"), "B", 35, 2, "Available", 1 },
                    { new Guid("87312397-5eda-491b-9a39-420cce3dc985"), "B", 5, 2, "Available", 1 },
                    { new Guid("8b542569-67ee-4da2-84c6-b8fc184f0cbd"), "B", 14, 2, "Available", 1 },
                    { new Guid("8bf8ace6-349e-4dc4-94b9-1182d586c5cb"), "B", 24, 2, "Available", 1 },
                    { new Guid("8e8ee5e0-9087-4a55-8aeb-1d13f3d271ea"), "B", 11, 2, "Available", 1 },
                    { new Guid("921ffa38-902b-4daa-9ebb-31b4a65eec5c"), "B", 16, 2, "Available", 1 },
                    { new Guid("92ce1191-9ed8-42f4-9631-8350e5ff7bc3"), "A", 47, 1, "Available", 1 },
                    { new Guid("95d255bf-283c-4197-86ee-f9821d5a82fb"), "B", 25, 2, "Available", 1 },
                    { new Guid("9925217d-98e7-49c5-b493-ae23975ac683"), "B", 28, 2, "Available", 1 },
                    { new Guid("9e9ac889-d64c-4040-bfcb-359bc853e202"), "B", 48, 2, "Available", 1 },
                    { new Guid("9ef8f9f9-6840-47cc-9022-2422a72af5d9"), "A", 32, 1, "Available", 1 },
                    { new Guid("a14f0e35-344d-46e3-937d-ec6c9db96f4a"), "B", 39, 2, "Available", 1 },
                    { new Guid("a2719cd2-8a39-4390-93b8-2675434e3154"), "A", 39, 1, "Available", 1 },
                    { new Guid("a67a2708-3217-4021-90f5-02fdc2f8754b"), "A", 35, 1, "Available", 1 },
                    { new Guid("a6b1c4af-cef9-40ac-9e5e-de6e96ebf72f"), "B", 2, 2, "Available", 1 },
                    { new Guid("a84b850f-c891-41f3-b22e-3e6aedad3856"), "B", 36, 2, "Available", 1 },
                    { new Guid("a8cb8fdf-7f9e-4795-a493-344fcbc49178"), "B", 12, 2, "Available", 1 },
                    { new Guid("aaff7c80-8a22-4fe3-abf5-1c0b757f3671"), "B", 38, 2, "Available", 1 },
                    { new Guid("ace5b963-786d-4dd8-9b4c-716c29333732"), "A", 26, 1, "Available", 1 },
                    { new Guid("af232d53-1f86-402b-a865-c59c00c22d03"), "A", 19, 1, "Available", 1 },
                    { new Guid("b0d8ec08-8f93-4adf-86c9-da04026c4205"), "A", 4, 1, "Available", 1 },
                    { new Guid("b3fe5e0d-3c1b-47f2-975b-01591a49b085"), "A", 41, 1, "Available", 1 },
                    { new Guid("b5c3e44b-5961-41fb-96d5-a90abb31e2cf"), "A", 7, 1, "Available", 1 },
                    { new Guid("b74b40a0-387e-414e-b4eb-8e9d8ef6ade1"), "B", 32, 2, "Available", 1 },
                    { new Guid("bbfc54f8-8d13-468f-bed5-226918de17ba"), "A", 5, 1, "Available", 1 },
                    { new Guid("bd1f6601-5589-4e53-abe5-67b908fe8868"), "A", 16, 1, "Available", 1 },
                    { new Guid("c4a684c8-0bc1-4d5b-b5b2-c2f3f4e12fe2"), "A", 21, 1, "Available", 1 },
                    { new Guid("c904e1f5-160a-4935-ba16-2458ccab4f3b"), "B", 46, 2, "Available", 1 },
                    { new Guid("caa31f84-0a31-466e-b545-3158c695b714"), "A", 2, 1, "Available", 1 },
                    { new Guid("cd6db3e2-decb-4aa2-ab63-115304440e9a"), "B", 33, 2, "Available", 1 },
                    { new Guid("d026cd7c-59da-4386-8098-7e918e2050d2"), "A", 33, 1, "Available", 1 },
                    { new Guid("d1e47a4d-e32e-4ba2-a4f3-e535daa83226"), "A", 46, 1, "Available", 1 },
                    { new Guid("d2b158a0-1e46-4225-b22c-0b7afe08a689"), "B", 40, 2, "Available", 1 },
                    { new Guid("d2f4157c-7ef5-4da2-bf8b-311c69879c59"), "A", 8, 1, "Available", 1 },
                    { new Guid("d7144f53-c713-4825-a929-b57eb4b38099"), "B", 37, 2, "Available", 1 },
                    { new Guid("e128a418-85d5-4e59-99ef-188bbc1cc41f"), "B", 15, 2, "Available", 1 },
                    { new Guid("e1c877da-0626-4444-88fb-8871ca5a29a7"), "A", 50, 1, "Available", 1 },
                    { new Guid("e298f162-e831-4486-a3c8-37682df2018b"), "A", 18, 1, "Available", 1 },
                    { new Guid("e41c0e2a-512a-4568-920e-907f2dbedb22"), "A", 42, 1, "Available", 1 },
                    { new Guid("e50d3aee-a247-4909-b50a-feafd24ed9c3"), "A", 30, 1, "Available", 1 },
                    { new Guid("e56c434a-eb1a-49ed-9073-2512e431a867"), "A", 17, 1, "Available", 1 },
                    { new Guid("ee342f9d-446f-426e-ac60-da30bf6ed1ea"), "B", 20, 2, "Available", 1 },
                    { new Guid("f83afdea-8ce7-4770-a436-ec8af53cf964"), "A", 3, 1, "Available", 1 },
                    { new Guid("fa7e6d64-f4c7-4da3-b2e8-4f12baa39bcf"), "B", 50, 2, "Available", 1 },
                    { new Guid("fabf43e7-039c-48bc-bea0-b0dd792352c1"), "A", 6, 1, "Available", 1 },
                    { new Guid("fc394933-f143-4b32-b49a-14416a5d1b16"), "B", 27, 2, "Available", 1 },
                    { new Guid("fcceaf93-b51e-4f28-be03-adc83679c29a"), "B", 21, 2, "Available", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SeatId",
                table: "Reservations",
                column: "SeatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_SectorId",
                table: "Seats",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sectors_EventId",
                table: "Sectors",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Sectors");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
