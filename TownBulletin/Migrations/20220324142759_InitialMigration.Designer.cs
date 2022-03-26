﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TownBulletin.Models;

#nullable disable

namespace TownBulletin.Migrations
{
    [DbContext(typeof(TownBulletinDbContext))]
    [Migration("20220324142759_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("TownBulletin.Models.EventTest", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("JsonData")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("EventTests");
                });

            modelBuilder.Entity("TownBulletin.Models.Module", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EntryMethod")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ExecutionOrder")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("StopEventExecution")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Event");

                    b.ToTable("Modules");
                });

            modelBuilder.Entity("TownBulletin.Models.Setting", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("TownBulletin.Models.Vector", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("Vectors");
                });
#pragma warning restore 612, 618
        }
    }
}
