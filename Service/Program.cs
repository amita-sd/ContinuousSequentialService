using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmitaAttribute = amita.primitives.net.Attribute;

namespace Service
{
    public static class Program
    {
        private const string Url = "http://localhost:8080/";

        private static SequentialService sequential_service = new SequentialService();
        

        public static void Main()
        {
            using (WebApp.Start(Url, ConfigureApplication))
            {
                Console.WriteLine("Listening at {0}", Url);

                Console.ReadLine();
            }
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

                try
                {
                    return Task.Run(async () =>
                    {
                        var a = await new ServiceController(sequential_service).request(await build_request_attributes(context));

                        return context.Response.WriteAsync(a);
                    });
                }
                catch(Exception e)
                {
                    return context.Response.WriteAsync("500");
                }

            });
        }

        private static async Task<IEnumerable<AmitaAttribute>> build_request_attributes(IOwinContext context)
        {
            //var formData = await context.Request.ReadFormAsync();
            //formData.First()

            return
                context.Request.Query
                .Select(convert_to_attribute);
        }

        private static AmitaAttribute convert_to_attribute(KeyValuePair<string, string[]> key_value)
        {
            if (key_value.Value.Length == 0)
            {
                return AmitaAttribute.create_value(key_value.Key, "");
            }

            if (key_value.Value.Length == 1)
            {
                return AmitaAttribute.create_value(key_value.Key, key_value.Value[0]);
            }

            return AmitaAttribute
                .create_collection(key_value.Key, 
                                    key_value.Value
                                                .Select((v, i) => AmitaAttribute.create_value(i.ToString(), v)));
        }
    }
}
