using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ServiceClient
{
    public static class Safely
    {
        public static async Task<T> execute<T>(Func<Task<T>> to_do, string method_name = "Untitled") where T : class
        {
            T payload = default(T);

            try
            {
                using (var profiler = new CodeTimer(method_name))
                {
                    payload = await to_do();
                }
            }
            catch (Exception e)
            {
                record_it(e);
            }

            return payload;
        }

        public static async Task execute(Func<Task> to_do, string method_name = "Untitled")
        {
            try
            {
                using (var profiler = new CodeTimer(method_name))
                {
                    await to_do();
                }
            }
            catch (Exception e)
            {
                record_it(e);
            }
        }

        public static T execute<T>(Func<T> to_do, string method_name = "Untitled") where T : class
        {
            T payload = default(T);

            try
            {
                using (var profiler = new CodeTimer(method_name))
                {
                    payload = to_do();
                }
            }
            catch (Exception e)
            {
                record_it(e);
            }

            return payload;
        }

        public static void execute(Action to_do, string method_name = "Untitled")
        {
            try
            {
                using (var profiler = new CodeTimer(method_name))
                {
                    to_do();
                }
            }
            catch (Exception e)
            {
                record_it(e);
            }
        }

        private static void record_it(Exception e)
        {
            Debug.WriteLine("Safely-Execute-Exception: " + e.Message);
            Debug.WriteLine("Safely-Execute-Inner-Exception: " + e.InnerException);
        }
    }
}

