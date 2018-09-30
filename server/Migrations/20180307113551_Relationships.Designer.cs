﻿// <auto-generated />

using System;
using DocumentAnnotation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
namespace DocumentAnnotation.Migrations
{
    [DbContext(typeof(AnnotationContext))]
    [Migration("20180307113551_Relationships")]
    partial class Relationships
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

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

            modelBuilder.Entity("server.Models.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("server.Models.DocumentAnnotation", b =>
                {
                    b.Property<int>("DocumentAnnotationId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("TextId");

                    b.Property<string>("UserId");

                    b.HasKey("DocumentAnnotationId");

                    b.HasIndex("TextId");

                    b.HasIndex("UserId");

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

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("server.Models.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("server.Models.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("server.Models.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("server.Models.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("server.Models.Annotation", b =>
                {
                    b.HasOne("server.Models.DocumentAnnotation", "DocumentAnnotation")
                        .WithMany("Annotations")
                        .HasForeignKey("DocumentAnnotationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("server.Models.DocumentAnnotation", b =>
                {
                    b.HasOne("server.Models.TextData", "Text")
                        .WithMany()
                        .HasForeignKey("TextId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("server.Models.AppUser", "User")
                        .WithMany("DocumentAnnotations")
                        .HasForeignKey("UserId");
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
