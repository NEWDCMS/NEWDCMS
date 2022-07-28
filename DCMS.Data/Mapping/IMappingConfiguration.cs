using Microsoft.EntityFrameworkCore;

namespace DCMS.Data.Mapping
{
    public partial interface IMappingConfiguration
    {
        void ApplyConfiguration(ModelBuilder modelBuilder);
    }
}