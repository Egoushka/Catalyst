using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalyst.ExampleModule.Example.Domain.Models;

public class ServiceOrganizationConfiguration : IEntityTypeConfiguration<ServiceOrganization>
{
    public void Configure(EntityTypeBuilder<ServiceOrganization> builder)
    {
        builder.ToTable("ServiceOrganizations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}