using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DocumentAnnotation.Migrations
{
    public partial class SingleLocationInHighlight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "End_BookNumber",
                table: "Highlights");

            migrationBuilder.DropColumn(
                name: "End_GroupNumber",
                table: "Highlights");

            migrationBuilder.DropColumn(
                name: "End_SectionNumber",
                table: "Highlights");

            migrationBuilder.DropColumn(
                name: "End_WordNumber",
                table: "Highlights");

            migrationBuilder.RenameColumn(
                name: "Start_WordNumber",
                table: "Highlights",
                newName: "Location_WordNumber");

            migrationBuilder.RenameColumn(
                name: "Start_SectionNumber",
                table: "Highlights",
                newName: "Location_SectionNumber");

            migrationBuilder.RenameColumn(
                name: "Start_GroupNumber",
                table: "Highlights",
                newName: "Location_GroupNumber");

            migrationBuilder.RenameColumn(
                name: "Start_BookNumber",
                table: "Highlights",
                newName: "Location_BookNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location_WordNumber",
                table: "Highlights",
                newName: "Start_WordNumber");

            migrationBuilder.RenameColumn(
                name: "Location_SectionNumber",
                table: "Highlights",
                newName: "Start_SectionNumber");

            migrationBuilder.RenameColumn(
                name: "Location_GroupNumber",
                table: "Highlights",
                newName: "Start_GroupNumber");

            migrationBuilder.RenameColumn(
                name: "Location_BookNumber",
                table: "Highlights",
                newName: "Start_BookNumber");

            migrationBuilder.AddColumn<int>(
                name: "End_BookNumber",
                table: "Highlights",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "End_GroupNumber",
                table: "Highlights",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "End_SectionNumber",
                table: "Highlights",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "End_WordNumber",
                table: "Highlights",
                nullable: false,
                defaultValue: 0);
        }
    }
}
