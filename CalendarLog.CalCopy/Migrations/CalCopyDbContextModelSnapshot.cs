﻿// <auto-generated />
using System;
using CalendarLog.CalCopy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CalendarLog.CalCopy.Migrations
{
    [DbContext(typeof(CalCopyDbContext))]
    partial class CalCopyDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9");

            modelBuilder.Entity("CalendarLog.CalCopy.Models.Settings", b =>
                {
                    b.Property<int>("SettingsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("APIKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("APIUrl")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastModifiedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("MasterTemplateFile")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProoferInitials")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProofingFolder")
                        .HasColumnType("TEXT");

                    b.Property<string>("SecretKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("WorkingCalendarFolder")
                        .HasColumnType("TEXT");

                    b.HasKey("SettingsId");

                    b.ToTable("Settings");
                });
#pragma warning restore 612, 618
        }
    }
}
