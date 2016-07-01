using Domain;
using System.Linq;

namespace Persistence
{
    public class PersistenceService
    {
        public void persist(EmployeeScheduleStartDate source)
        {
            // model
            var day = source.start_date;

            repository.remove_all();//This is important because we have no Entity IDs yet

            // serialise and persist
            foreach(var attr in serialise.Day.create_memento(day))
            {
                repository.add(attr.ToEFAttribute());
            }

            repository.commit();      
        }

        public EmployeeScheduleStartDate load()
        {
            //deserialise
            return 
                serialise
                .Day
                .create_day(repository.Entities.ToList().Select(i => i.ToDataAttribute()))
                .match(
                s => {
                    return new EmployeeScheduleStartDate(s);
                },
                e => {
                    var day = new domain.Day("A new day", time.Dates.today(), new domain.DiaryEntry[0]);
                    return new EmployeeScheduleStartDate(day);
                });
            
        }

        public Repository repository = new Repository();       
        
    }

}
