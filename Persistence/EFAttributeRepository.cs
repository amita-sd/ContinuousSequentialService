using System.Data.Entity;
using System.Linq;

namespace Persistence
{
    public class Repository
    {
        public Repository()
        {
            context = new AppDbContext();
        }
        
        public IQueryable<EFAttribute> Entities { get { return context.Atributes; } }

        /// <inheritdoc/>
        public void add(EFAttribute entity)
        {
            context.Atributes.Add(entity);
        }

        public void remove(EFAttribute entity)
        {
            context.Atributes.Attach(entity);
            context.Entry(entity).State = EntityState.Deleted;
            context.Atributes.Remove(entity);
        }

        public void remove_all()
        {
            foreach (var item in context.Atributes)
            {
                remove(item);
            }
        }

        public void commit()
        {
            context.SaveChanges();
        }

        private readonly AppDbContext context;

    }
}
