﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UroMeter.DataAccess;

#nullable disable

namespace UroMeter.DataAccess.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240710191110_UpdateRecordAt2")]
    partial class UpdateRecordAt2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("UroMeter.DataAccess.Models.Device", b =>
                {
                    b.Property<string>("MacAddress")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("LastSeen")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("PatientId")
                        .HasColumnType("integer");

                    b.HasKey("MacAddress");

                    b.HasIndex("PatientId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("UroMeter.DataAccess.Models.Record", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CheckUpAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("Finished")
                        .HasColumnType("boolean");

                    b.Property<int>("PatientId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("RecordAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.ToTable("Records");
                });

            modelBuilder.Entity("UroMeter.DataAccess.Models.RecordData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("RecordAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("RecordId")
                        .HasColumnType("integer");

                    b.Property<double>("Volume")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("RecordId");

                    b.ToTable("RecordDatas");
                });

            modelBuilder.Entity("UroMeter.DataAccess.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("BirthDay")
                        .HasColumnType("date");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("UroMeter.DataAccess.Models.Device", b =>
                {
                    b.HasOne("UroMeter.DataAccess.Models.User", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId");

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("UroMeter.DataAccess.Models.Record", b =>
                {
                    b.HasOne("UroMeter.DataAccess.Models.User", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("UroMeter.DataAccess.Models.RecordData", b =>
                {
                    b.HasOne("UroMeter.DataAccess.Models.Record", "Record")
                        .WithMany()
                        .HasForeignKey("RecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Record");
                });
#pragma warning restore 612, 618
        }
    }
}
