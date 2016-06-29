using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;

namespace ServiceClient
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public async Task plain_request()
        {
            var page_string = await EmptyRequest.execute();
        }

        [TestMethod]
        public async Task add_shift()
        {
            var page_string = await AddShift.execute(new AddShiftRequest("hooli"));
        }

        [TestMethod]
        public async Task add_shift_n_times()
        {
            using (var profiler = new CodeTimer("add_shift_n_times"))
            {
                var tasks =
                Enumerable
                    .Range(1, 1000)
                    .Select(i => add_shift());

                await Task.WhenAll(tasks);
            }
        }

        [TestMethod]
        public async Task plain_request_n_times()
        {
            using (var profiler = new CodeTimer("plain_request_n_times"))
            {
                var tasks =
                Enumerable
                    .Range(1, 1280)
                    .Select(i => plain_request());

                await Task.WhenAll(tasks);
            }
        }
    }
}
