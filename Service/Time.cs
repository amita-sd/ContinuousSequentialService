using System;
namespace Service
{
    public class Date
    {

        public int year { get; private set; }
        public int month { get; private set; }
        public int day { get; private set; }

        public Date
                (int year
                , int month
                , int day)
        {


            new DateTime(year, month, day);

            this.year = year;
            this.month = month;
            this.day = day;
        }

    }

    public static class ClockTimes
    {

        public static bool SecondsAreWithinTwentyfourHourClock
                     (int seconds)
        {

            return seconds >= 0 && seconds <= 60 * 60 * 24;
        }
    }
}
