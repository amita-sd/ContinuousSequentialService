namespace ServiceClient
{
    using System;
    using System.Diagnostics;

    public class CodeTimer : IDisposable
    {
        private Stopwatch _stopWatch;
        private string _title;

        public CodeTimer() : this("Untitled")
        { }

        public CodeTimer(string title)
        {
            _title = title;

            Debug.WriteLine(title + "_started");

            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        private string elapsed_time_in_seconds => (_stopWatch.ElapsedMilliseconds / 1000.0).ToString();

        public void Print(string comment)
        {
            Debug.WriteLine("{0}: {1} @ {2} seconds", _title, comment, elapsed_time_in_seconds);
        }

        public void Dispose()
        {
            _stopWatch.Stop();

            Debug.WriteLine("{0}: completed  @ {1} seconds", _title, elapsed_time_in_seconds);

            _stopWatch = null;
            _title = null;
        }
    }
}