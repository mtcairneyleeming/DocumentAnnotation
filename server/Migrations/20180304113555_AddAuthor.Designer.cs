﻿// <auto-generated />

using DocumentAnnotation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
namespace DocumentAnnotation.Migrations
{
    [DbContext(typeof(AnnotationContext))]
    [Migration("20180304113555_AddAuthor")]
    partial class AddAuthor
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("server.Models.Annotation", b =>
                {
                    b.Property<int>("AnnotationId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body");

                    b.Property<int>("DocumentAnnotationId");

                    b.Property<string>("Title");

                    b.HasKey("AnnotationId");

                    b.HasIndex("DocumentAnnotationId");

                    b.ToTable("Annotations");
                });

            modelBuilder.Entity("server.Models.Document", b =>
                {
                    b.Property<int>("DocumentAnnotationId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("TextId");

                    b.Property<string>("UserId");

                    b.HasKey("DocumentAnnotationId");

                    b.ToTable("DocumentAnnotations");
                });

            modelBuilder.Entity("server.Models.Highlight", b =>
                {
                    b.Property<int>("HighlightId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AnnotationId");

                    b.HasKey("HighlightId");

                    b.HasIndex("AnnotationId");

                    b.ToTable("Highlights");
                });

            modelBuilder.Entity("server.Models.TextData", b =>
                {
                    b.Property<int>("TextId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Abbreviation");

                    b.Property<string>("Author");

                    b.Property<string>("Identifier");

                    b.Property<string>("Title");

                    b.HasKey("TextId");

                    b.ToTable("Texts");
                });

            modelBuilder.Entity("server.Models.Annotation", b =>
                {
                    b.HasOne("server.Models.Document", "Document")
                        .WithMany("Annotations")
                        .HasForeignKey("DocumentAnnotationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("server.Models.Highlight", b =>
                {
                    b.HasOne("server.Models.Annotation", "Annotation")
                        .WithMany("Highlights")
                        .HasForeignKey("AnnotationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("server.Models.Location", "End", b1 =>
                        {
                            b1.Property<int?>("HighlightId");

                            b1.Property<int>("BookNumber");

                            b1.Property<int>("Character");

                            b1.Property<int>("GroupNumber");

                            b1.Property<int>("LocationId");

                            b1.Property<int>("SectionNumber");

                            b1.ToTable("Highlights");

                            b1.HasOne("server.Models.Highlight")
                                .WithOne("End")
                                .HasForeignKey("server.Models.Location", "HighlightId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne("server.Models.Location", "Start", b1 =>
                        {
                            b1.Property<int>("HighlightId");

                            b1.Property<int>("BookNumber");

                            b1.Property<int>("Character");

                            b1.Property<int>("GroupNumber");

                            b1.Property<int>("LocationId");

                            b1.Property<int>("SectionNumber");

                            b1.ToTable("Highlights");

                            b1.HasOne("server.Models.Highlight")
                                .WithOne("Start")
                                .HasForeignKey("server.Models.Location", "HighlightId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
