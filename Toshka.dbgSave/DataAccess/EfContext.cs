using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Toshka.dbgSave.Model;

namespace Toshka.dbgSave.DataAccess
{
    public class EfContext : DbContext
    {
        public EfContext(DbContextOptions<EfContext> options) : base(options)
        {
            //
        }

        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<ModelInput> ModelsInput { get; set; }
        public DbSet<ModelOutput> ModelsOutput { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<ModelInput> modelsInputBuilder = modelBuilder.Entity<ModelInput>();
            modelsInputBuilder.HasKey(at => at.Id);

            EntityTypeBuilder<TelegramUser> tuBuilder = modelBuilder.Entity<TelegramUser>();
            tuBuilder.HasKey(at => at.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}