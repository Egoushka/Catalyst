using System.Net.NetworkInformation;
using Catalyst.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Catalyst.ExampleModule.Example.Domain.Models;

public class ExampleEntityConfiguration : IEntityTypeConfiguration<ExampleEntity>
{
    public void Configure(EntityTypeBuilder<ExampleEntity> builder)
    {
        var macConverter = new ValueConverter<MacAddress, PhysicalAddress>(
            mac => mac.Address,
            phys => new MacAddress(phys)
        );

        // 2) Change-tracking & index/key comparison
        var macComparer = new ValueComparer<MacAddress>(
            (a, b) => a.Equals(b),
            a => a.GetHashCode(),
            a => new MacAddress(a.Address)
        );

        builder.ToTable("ExampleEntitys");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();


        builder.Property(x => x.SerialNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CreatedDateTime)
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedDateTime)
            .HasDefaultValueSql("NOW()");

        builder.Property(d => d.GeneralStatus)
            .HasConversion<byte>()
            .HasColumnName("GeneralStatusId")
            .HasDefaultValue(default)
            .IsRequired();

        builder.HasOne(x => x.Type)
            .WithMany()
            .HasForeignKey(x => x.TypeId)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.SerialNumber)
            .IsUnique()
            .HasDatabaseName("UX_Displays_SerialNumber");
    }
}