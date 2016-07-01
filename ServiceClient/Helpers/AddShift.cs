using Refit;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceClient
{

    #region AddShift
    public static class AddShift
    {
        public static async Task<string> execute(AddShiftRequest request)
        {
            return await
                Safely.execute(async () =>
                     await RestService
                        .For<IAddShift>(HttpClientProvider.new_instance)
                        .execute(request.title), "AddShift");
        }
    }

    public interface IAddShift
    {
        [Get("/add-shift")]
        Task<string> execute(string title);
    }

    public class AddShiftRequest
    {
        public string title { get; private set; }

        public AddShiftRequest(string the_title)
        {
            title = the_title;
        }
    }

    #endregion

    #region EmptyRequest

    public static class EmptyRequest
    {
        public static async Task<string> execute()
        {
            return await
                Safely.execute(async () =>
                     await RestService
                        .For<IEmptyRequest>(HttpClientProvider.new_instance)
                        .execute(), "EmptyRequest");
        }
    }

    public interface IEmptyRequest
    {
        [Get("/empty-request")]
        Task<string> execute();
    }


    public class HttpClientProvider
    {
        public static HttpClient new_instance
        {
            get
            {
                return new HttpClient(new HttpClientHandler()) { BaseAddress = new Uri("http://localhost:8080"), Timeout = TimeSpan.FromDays(2.0) };
            }
        }
    }

    #endregion

}
