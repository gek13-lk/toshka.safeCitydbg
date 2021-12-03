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
        public DbSet<Event> Events { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<ModelInput> ModelsInput { get; set; }
        public DbSet<ModelOutput> ModelsOutput { get; set; }
        public DbSet<Garbage> Garbages { get; set; }
        public DbSet<CameraFilterMarkup> CameraFilterMarkups { get; set; }
        public DbSet<Export> Exports { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<ModelInput> modelsInputBuilder = modelBuilder.Entity<ModelInput>();
            modelsInputBuilder.HasKey(at => at.Id);

            EntityTypeBuilder<TelegramUser> tuBuilder = modelBuilder.Entity<TelegramUser>();
            tuBuilder.HasKey(at => at.Id);

            EntityTypeBuilder<Camera> camerasBuilder = modelBuilder.Entity<Camera>();
            camerasBuilder.HasKey(at => at.Id);

            EntityTypeBuilder<Event> eventsBuilders = modelBuilder.Entity<Event>();
            eventsBuilders.HasKey(at => at.Id);

            EntityTypeBuilder<Garbage> gbBuilders = modelBuilder.Entity<Garbage>();
            gbBuilders.HasKey(at => at.Id);

            EntityTypeBuilder<CameraFilterMarkup> cfmBuilders = modelBuilder.Entity<CameraFilterMarkup>();
            cfmBuilders.HasKey(at => at.Id);

            EntityTypeBuilder<Export> exportBuilders = modelBuilder.Entity<Export>();
            exportBuilders.HasKey(at => at.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}