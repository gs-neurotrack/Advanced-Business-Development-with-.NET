using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeuroTrack.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GS_LIMITS",
                columns: table => new
                {
                    ID_LIMITS = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    LIMIT_HOURS = table.Column<byte>(type: "NUMBER(2)", nullable: false),
                    LIMIT_MEETINGS = table.Column<byte>(type: "NUMBER(2)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GS_LIMITS", x => x.ID_LIMITS);
                });

            migrationBuilder.CreateTable(
                name: "GS_ROLE",
                columns: table => new
                {
                    ID_ROLE = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ROLE_NAME = table.Column<string>(type: "VARCHAR2(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GS_ROLE", x => x.ID_ROLE);
                });

            migrationBuilder.CreateTable(
                name: "GS_STATUS_RISK",
                columns: table => new
                {
                    ID_STATUS_RISK = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    STATUS_NAME_RISK = table.Column<string>(type: "VARCHAR2(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GS_STATUS_RISK", x => x.ID_STATUS_RISK);
                });

            migrationBuilder.CreateTable(
                name: "GS_USERS",
                columns: table => new
                {
                    ID_USER = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME_USER = table.Column<string>(type: "VARCHAR2(100)", nullable: false),
                    EMAIL_USER = table.Column<string>(type: "VARCHAR2(255)", nullable: false),
                    PASSWORD_USER = table.Column<string>(type: "VARCHAR2(255)", nullable: false),
                    STATUS = table.Column<string>(type: "CHAR(1)", nullable: false),
                    ID_ROLE = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false),
                    ID_LIMITS = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GS_USERS", x => x.ID_USER);
                    table.ForeignKey(
                        name: "FK_GS_LIMITS_GS_USERS",
                        column: x => x.ID_LIMITS,
                        principalTable: "GS_LIMITS",
                        principalColumn: "ID_LIMITS",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GS_ROLE_GS_USERS",
                        column: x => x.ID_ROLE,
                        principalTable: "GS_ROLE",
                        principalColumn: "ID_ROLE",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GS_DAILY_LOGS",
                columns: table => new
                {
                    ID_LOG = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    WORK_HOURS = table.Column<byte>(type: "NUMBER(2)", nullable: false),
                    MEETINGS = table.Column<byte>(type: "NUMBER(2)", nullable: false),
                    LOG_DATE = table.Column<DateTime>(type: "DATE", nullable: false),
                    ID_USER = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GS_DAILY_LOGS", x => x.ID_LOG);
                    table.ForeignKey(
                        name: "FK_GS_USER_GS_DAILY_LOGS",
                        column: x => x.ID_USER,
                        principalTable: "GS_USERS",
                        principalColumn: "ID_USER",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GS_SCORES",
                columns: table => new
                {
                    ID_SCORES = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DATE_SCORE = table.Column<DateTime>(type: "DATE", nullable: false),
                    SCORE_VALUE = table.Column<float>(type: "FLOAT(4)", nullable: false),
                    TIME_RECOMMENDATION = table.Column<byte>(type: "NUMBER(2)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "DATE", nullable: false),
                    ID_STATUS_RISK = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false),
                    ID_USER = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false),
                    ID_LOG = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GS_SCORES", x => x.ID_SCORES);
                    table.ForeignKey(
                        name: "FK_GS_DAILY_LOGS_GS_SCORES",
                        column: x => x.ID_LOG,
                        principalTable: "GS_DAILY_LOGS",
                        principalColumn: "ID_LOG",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GS_STATUS_RISK_GS_SCORES",
                        column: x => x.ID_STATUS_RISK,
                        principalTable: "GS_STATUS_RISK",
                        principalColumn: "ID_STATUS_RISK",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GS_USER_GS_SCORES",
                        column: x => x.ID_USER,
                        principalTable: "GS_USERS",
                        principalColumn: "ID_USER",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GS_PREDICTIONS",
                columns: table => new
                {
                    ID_PREDICTION = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    STRESS_PREDICTED = table.Column<float>(type: "FLOAT(2)", nullable: false),
                    MESSAGE = table.Column<string>(type: "VARCHAR2(50)", nullable: false),
                    DATE_PREDICTED = table.Column<DateTime>(type: "DATE", nullable: false),
                    ID_USER = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false),
                    ID_SCORES = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false),
                    ID_STATUS_RISK = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GS_PREDICTIONS", x => x.ID_PREDICTION);
                    table.ForeignKey(
                        name: "FK_GS_SCORES_GS_PREDICTIONS",
                        column: x => x.ID_SCORES,
                        principalTable: "GS_SCORES",
                        principalColumn: "ID_SCORES",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GS_STATUS_RISK_GS_PREDICTIONS",
                        column: x => x.ID_STATUS_RISK,
                        principalTable: "GS_STATUS_RISK",
                        principalColumn: "ID_STATUS_RISK",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GS_USER_GS_PREDICTIONS",
                        column: x => x.ID_USER,
                        principalTable: "GS_USERS",
                        principalColumn: "ID_USER",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GS_DAILY_LOGS_ID_USER",
                table: "GS_DAILY_LOGS",
                column: "ID_USER");

            migrationBuilder.CreateIndex(
                name: "IX_GS_PREDICTIONS_ID_SCORES",
                table: "GS_PREDICTIONS",
                column: "ID_SCORES");

            migrationBuilder.CreateIndex(
                name: "IX_GS_PREDICTIONS_ID_STATUS_RISK",
                table: "GS_PREDICTIONS",
                column: "ID_STATUS_RISK");

            migrationBuilder.CreateIndex(
                name: "IX_GS_PREDICTIONS_ID_USER",
                table: "GS_PREDICTIONS",
                column: "ID_USER");

            migrationBuilder.CreateIndex(
                name: "IX_GS_SCORES_ID_LOG",
                table: "GS_SCORES",
                column: "ID_LOG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GS_SCORES_ID_STATUS_RISK",
                table: "GS_SCORES",
                column: "ID_STATUS_RISK");

            migrationBuilder.CreateIndex(
                name: "IX_GS_SCORES_ID_USER",
                table: "GS_SCORES",
                column: "ID_USER");

            migrationBuilder.CreateIndex(
                name: "IX_GS_USERS_ID_LIMITS",
                table: "GS_USERS",
                column: "ID_LIMITS",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GS_USERS_ID_ROLE",
                table: "GS_USERS",
                column: "ID_ROLE");

            migrationBuilder.CreateIndex(
                name: "UK_GS_USERS_EMAIL_USER",
                table: "GS_USERS",
                column: "EMAIL_USER",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GS_PREDICTIONS");

            migrationBuilder.DropTable(
                name: "GS_SCORES");

            migrationBuilder.DropTable(
                name: "GS_DAILY_LOGS");

            migrationBuilder.DropTable(
                name: "GS_STATUS_RISK");

            migrationBuilder.DropTable(
                name: "GS_USERS");

            migrationBuilder.DropTable(
                name: "GS_LIMITS");

            migrationBuilder.DropTable(
                name: "GS_ROLE");
        }
    }
}
