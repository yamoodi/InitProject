using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.Migrations
{
    /// <inheritdoc />
    public partial class seedingRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                 values: new object[] { Guid.NewGuid().ToString(), "user", "user".ToUpper(), Guid.NewGuid().ToString() });

            migrationBuilder.InsertData("AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                 values: new object[] { Guid.NewGuid().ToString(), "admin", "admin".ToUpper(), Guid.NewGuid().ToString() });

            migrationBuilder.InsertData("AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                 values: new object[] { Guid.NewGuid().ToString(), "supervisor", "supervisor".ToUpper(), Guid.NewGuid().ToString() });

            migrationBuilder.InsertData("AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                 values: new object[] { Guid.NewGuid().ToString(), "superadmin", "superadmin".ToUpper(), Guid.NewGuid().ToString() });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from [AspNetRoles]");
        }
    }
}
