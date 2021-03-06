// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheCrushinator.FitBit.Web.Models;

namespace TheCrushinator.FitBit.Web.Migrations
{
    [DbContext(typeof(FitbitContext))]
    [Migration("20210515042054_AddScaleEntryTable")]
    partial class AddScaleEntryTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TheCrushinator.FitBit.Web.Models.ScaleEntry", b =>
                {
                    b.Property<string>("EntryId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AMRKCal")
                        .HasColumnType("int");

                    b.Property<float>("BMI")
                        .HasColumnType("real");

                    b.Property<int>("BMRKCal")
                        .HasColumnType("int");

                    b.Property<float>("BodyFatPct")
                        .HasColumnType("real");

                    b.Property<float>("BoneMassKg")
                        .HasColumnType("real");

                    b.Property<DateTime>("FitBitUploadDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ImportDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<float>("MusclePct")
                        .HasColumnType("real");

                    b.Property<DateTime>("RecordDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<float>("WaterPct")
                        .HasColumnType("real");

                    b.Property<float>("WeightKg")
                        .HasColumnType("real");

                    b.HasKey("EntryId");

                    b.ToTable("BeurerWeightEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
