using amita.primitives.net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
    public class ServiceController
    {
        public async Task<string> request(IEnumerable<Attribute> request_attributes)
        {
            return await service.submit_request(request_attributes);
        }        

        public ServiceController(SequentialService the_service)
        {
            service = the_service;
        }

        private readonly SequentialService service;
    }
}
