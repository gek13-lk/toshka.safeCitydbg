using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Toshka.SafeCity.Model;

namespace Toshka.SafeCity.DataAccess
{
    public class EfContext : DbContext
    {
        public EfContext(DbContextOptions<EfContext> options) : base(options)
        {
            //
        }

        //public DbSet<CameraDistanceMarkup> CameraDistanceMarkups { get; set; }

        //public DbSet<CameraFilterMarkup> CameraFilterMarkups { get; set; }

        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<Camera> Cameras { get; set; }

        public DbSet<ModelInput> ModelsInput { get; set; }

        public DbSet<ModelOutput> ModelsOutput { get; set; }
        //public DbSet<FireEvent> FireEvents { get; set; }
        //public DbSet<AlgorithmSetting> AlgorithmSettings { get; set; }

        //public DbSet<RescueService> RescueServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<Camera> camerasBuilder = modelBuilder.Entity<Camera>();
            camerasBuilder.HasKey(at => at.Id);

            EntityTypeBuilder<ModelInput> modelsInputBuilder = modelBuilder.Entity<ModelInput>();
            modelsInputBuilder.HasKey(at => at.Id);

            //EntityTypeBuilder<ModelOutput> modelsOutputBuilder = modelBuilder.Entity<ModelOutput>();
            //modelsOutputBuilder.HasKey(at => at.Id);

            // EntityTypeBuilder<FireEvent> feBuilder = modelBuilder.Entity<FireEvent>();
            //feBuilder.HasKey(at => at.Id);

            //EntityTypeBuilder<AlgorithmSetting> asBuilder = modelBuilder.Entity<AlgorithmSetting>();
            //asBuilder.HasKey(at => at.Id);

            EntityTypeBuilder<TelegramUser> tuBuilder = modelBuilder.Entity<TelegramUser>();
            tuBuilder.HasKey(at => at.Id);

            //EntityTypeBuilder<RescueService> rsBuilder = modelBuilder.Entity<RescueService>();
            //rsBuilder.HasKey(at => at.Id);

            //EntityTypeBuilder<CameraDistanceMarkup> cdmBuilder = modelBuilder.Entity<CameraDistanceMarkup>();
            //cdmBuilder.HasKey(at => at.Id);

            //EntityTypeBuilder<CameraFilterMarkup> cfmBuilder = modelBuilder.Entity<CameraFilterMarkup>();
            //cfmBuilder.HasKey(at => at.Id);

            //modelBuilder.Entity<RescueService>().HasData(
            //new RescueService { Id = 1, Latitude = 51.717795, Longitude = 94.380083, Type = RescueService.Const_FireDepartment, Phone = "8 (394) 229-99-99", Title = "ГУ МЧС России по Республике Тыва" });

            //modelBuilder.Entity<RescueService>().HasData(
            //new RescueService { Id = 2, Latitude = 51.762750, Longitude = 94.350309, Type = RescueService.Const_FireDepartment, Phone = "8 (394) 229-99-52", Title = "Упр. МЧС" });

            //modelBuilder.Entity<RescueService>().HasData(
            //new RescueService { Id = 3, Latitude = 51.687003, Longitude = 94.481941, Type = RescueService.Const_FireDepartment, Phone = "8 (394) 233-09-90", Title = "Пожарная часть" });

            //modelBuilder.Entity<RescueService>().HasData(
            //new RescueService { Id = 4, Latitude = 51.861320, Longitude = 94.182968, Type = RescueService.Const_ForestryDepartment, Phone = "8 (683) 188-29-29", Title = "КЫЗЫЛСКОЕ ЛЕСНИЧЕСТВО" });

            //modelBuilder.Entity<RescueService>().HasData(
            //new RescueService { Id = 5, Latitude = 51.761383, Longitude = 94.589690, Type = RescueService.Const_ForestryDepartment, Phone = "8 (683) 188-29-29", Title = "Министерство сельского хозяйства и продовольствия Республики Тыва" });

            //modelBuilder.Entity<TelegramUser>().HasData(
            //new TelegramUser { Id = 1, ChatId = "111718761" });

            //modelBuilder.Entity<AlgorithmSetting>().HasData(
            //new AlgorithmSetting { Name = 1, Value = 1 });

            base.OnModelCreating(modelBuilder);
        }
    }
}