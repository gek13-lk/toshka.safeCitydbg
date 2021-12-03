﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Toshka.dbgSave.DataAccess;

namespace Toshka.dbgSave.Migrations
{
    [DbContext(typeof(EfContext))]
    [Migration("20211202180423_camera")]
    partial class camera
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Toshka.dbgSave.Model.Camera", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Deg")
                        .HasColumnType("text");

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Settings")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Cameras");
                });

            modelBuilder.Entity("Toshka.dbgSave.Model.ModelInput", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<float>("Export")
                        .HasColumnType("real");

                    b.Property<float>("Fullness")
                        .HasColumnType("real");

                    b.Property<DateTime>("RentalDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<float>("Weekend")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("ModelsInput");
                });

            modelBuilder.Entity("Toshka.dbgSave.Model.ModelOutput", b =>
                {
                    b.Property<float[]>("ForecastedRentals")
                        .HasColumnType("real[]");

                    b.Property<float[]>("LowerBoundRentals")
                        .HasColumnType("real[]");

                    b.Property<float[]>("UpperBoundRentals")
                        .HasColumnType("real[]");

                    b.ToTable("ModelsOutput");
                });

            modelBuilder.Entity("Toshka.dbgSave.Model.TelegramUser", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ChatId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("TelegramUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
