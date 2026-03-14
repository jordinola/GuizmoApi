using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GuizmoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guizmos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Msrp = table.Column<decimal>(type: "numeric(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guizmos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guizmos");
        }
    }
}
