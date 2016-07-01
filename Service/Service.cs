using Domain;
using Persistence;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmitaAttribute = amita.primitives.net.Attribute;

namespace Service
{
    public class SequentialService
    {
        public async Task<string> submit_request(IEnumerable<AmitaAttribute> request_attributes)
        {
            //Guard request here
            request_queue.Enqueue(new ServiceRequest(request_attributes, () => world, Enumerable.Empty<Permission>()));

            return await Task.FromResult(wait_for_response());
        }

        private string wait_for_response()
        {
            //elapsed time
            ServiceResponse resp;
            bool did_remove = false;
            do
            {

                did_remove = response_queue.TryDequeue(out resp);
            } while (!did_remove);//&& elapsed time > some threshold

            //persist
            persistence_service.persist(resp);

            return resp.adjusted_employee_schedules().ToString();
        }


        private ConcurrentQueue<ServiceRequest> request_queue = new ConcurrentQueue<ServiceRequest>();

        private ConcurrentQueue<ServiceResponse> response_queue = new ConcurrentQueue<ServiceResponse>();

        private EmployeeScheduleStartDate world;

        private PersistenceService persistence_service;


        public SequentialService()
        {
            persistence_service = new PersistenceService();

            world = persistence_service.load();

            initialise_service();            
        }

        private void initialise_service()
        {
            Task.Run(() =>
            {
                do
                {
                    ServiceRequest request;
                    if (request_queue.TryDequeue(out request))
                    {
                        var resp = Addshift.apply(request);
                        world = resp.adjusted_employee_schedules();
                        response_queue.Enqueue(resp);
                    }
                } while (true);

            });
        }
    }
}
