using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS4SecureGateway.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "As4MessageAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HttpStatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequestXml = table.Column<string>(type: "TEXT", nullable: false),
                    ResponseBody = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DocumentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ErrorCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ErrorDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FaultReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_As4MessageAudits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_As4MessageAudits_ActionType",
                table: "As4MessageAudits",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_As4MessageAudits_CreatedAtUtc",
                table: "As4MessageAudits",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_As4MessageAudits_DocumentId",
                table: "As4MessageAudits",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_As4MessageAudits_MessageId",
                table: "As4MessageAudits",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "As4MessageAudits");
        }
    }
}
