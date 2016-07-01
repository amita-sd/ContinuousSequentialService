using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace ServiceClients
{
    [TestClass]
    public class EFTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var persistence_service = new PersistenceService();


            var entity = new_instance("aloha");

            persistence_service.persist(entity);

            var loaded_entity = persistence_service.load();


            Assert.AreEqual(entity.start_date.summary, loaded_entity.start_date.summary);
        }


        private EmployeeScheduleStartDate new_instance(string title)
        {
            var day = new domain.Day(title, time.Dates.today(), new domain.DiaryEntry[0]);

            var new_entity = new EmployeeScheduleStartDate(day);

            return new_entity;

        }
    }
}
