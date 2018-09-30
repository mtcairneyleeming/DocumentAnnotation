using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DocumentAnnotation.Migrations
{
    public partial class WordUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Start_Character",
                table: "Highlights",
                newName: "Start_WordNumber");

            migrationBuilder.RenameColumn(
                name: "End_Character",
                table: "Highlights",
                newName: "End_WordNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Start_WordNumber",
                table: "Highlights",
                newName: "Start_Character");

            migrationBuilder.RenameColumn(
                name: "End_WordNumber",
                table: "Highlights",
                newName: "End_Character");
        }
    }
}