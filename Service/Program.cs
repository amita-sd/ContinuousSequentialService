using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public static class Program
    {
        private const string Url = "http://localhost:8080/";

        private static ConcurrentQueue<ServiceRequest> request_queue = new ConcurrentQueue<ServiceRequest>();

        private static ConcurrentQueue<ServiceResponse> response_queue = new ConcurrentQueue<ServiceResponse>();

        private static EmployeeSchedules world = new EmployeeSchedules(0);

        public static void Main()
        {
            Task.Run(() =>
            {
                init_service_layer();
            });

            using (WebApp.Start(Url, ConfigureApplication))
            {
                Console.WriteLine("Listening at {0}", Url);

                Console.ReadLine();
            }
        }

        private static void init_service_layer()
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
        }

        private static void ConfigureApplication(IAppBuilder app)
        {
            app.Run(context =>
            {

                //Console.WriteLine(
                //    "Request \"{0}\" from: {1}:{2}",
                //    context.Request.Path,
                //    context.Request.RemoteIpAddress,
                //    context.Request.RemotePort);

                context.Response.ContentType = "text/plain";



                if (!context.Request.Path.Value.Contains("add-shift"))
                {
                    return context.Response.WriteAsync("404");
                }

                return Task.Run(async () =>
                {
                    var a = await controller(await build_request_attributes(context));

                    return context.Response.WriteAsync(a);
                });

            });
        }

        private static async Task<IEnumerable<RequestAttribute>> build_request_attributes(IOwinContext context)
        {
            var formData = await context.Request.ReadFormAsync();
            //formData.First().

            return
                context.Request.Query
                .Select(i => new RequestAttribute("NewShiftRequest", i.Key, determine_value_type(i.Value)));

        }

        private static IAttributeValue determine_value_type(string[] request_params)
        {
            if (request_params.Length == 0)
            {
                return new ASimpleValue("");
            }

            if (request_params.Length == 1)
            {
                return new ASimpleValue(request_params[0]);
            }

            return new ACollectionValue(request_params.Select(rp => new ASimpleValue(rp)));
        }

        private static async Task<string> controller(IEnumerable<RequestAttribute> request_attributes)
        {
            request_queue.Enqueue(new ServiceRequest(request_attributes, () => world, Enumerable.Empty<Permission>()));
            return await Task.FromResult(service_layer());
        }


        private static string service_layer()
        {
            //elapsed time
            ServiceResponse resp;
            bool did_remove = false;
            do
            {

                did_remove = response_queue.TryDequeue(out resp);
            } while (!did_remove);//&& elapsed time > some threshold

            return resp.adjusted_employee_schedules().count.ToString();
        }


    }
}
