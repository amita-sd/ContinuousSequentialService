using System.Data.Entity;

namespace Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
            : base("name=SequentialServiceContext")
        {

            InitialiseDatabase();
        }

        public DbSet<EFAttribute> Atributes { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder
                .Entity<CollectionEFAttribute>()
                .HasMany(e => e.attributes)
                .WithMany();
        }


        private void InitialiseDatabase()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<AppDbContext>());
            Database.Initialize(false);
        }
    }
}