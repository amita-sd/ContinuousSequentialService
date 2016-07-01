using amita.primitives.net;
using System;
using System.Collections.Generic;
using System.Linq;
using DataAttribute = amita.primitives.net.Attribute;

namespace domain
{

    public class Day
    {

        public string summary { get; private set; }

        public time.Date date { get; private set; }

        public IEnumerable<DiaryEntry> diary { get; private set; }

        public Day
                (string summary
                , time.Date date
                , IEnumerable<DiaryEntry> diary)
        {

            this.summary = summary;
            this.date = date;
            this.diary = diary;
        }
    }


    public class DiaryEntry
    {

        public time.ClockTime start_time { get; private set; }

        public DiaryEntry
                (time.ClockTime start_time)
        {

            this.start_time = start_time;
        }
    }
}


namespace serialise
{

    public static class Day
    {

        public class SummarySerialisationError : ResultError { }

        public class DateSerialisationError : ResultError { }

        public class DiarySerialisationError : ResultError { }


        public static IEnumerable<DataAttribute> create_memento
                                              (domain.Day day)
        {

            return new DataAttribute[] {
                         DataAttribute.create_value( "summary", day.summary ),
                         DataAttribute.create_collection( "date", time.serialisation.Date.create_attribute_memento( day.date ) ),
                         DataAttribute.create_collection( "diary",  day.diary.Select((de, i) => DataAttribute.create_collection(i.ToString(), DiaryEntry.create_memento(de))))
                    };
        }

        private class AggregatedDiaryEntryResults
        {

            public AggregatedDiaryEntryResults add_result
                                                (Result<domain.DiaryEntry> result)
            {

                result.match(
                   success: entries.Add
                  , error: errors.AddRange
                );
                return this;
            }

            public Result<IEnumerable<domain.DiaryEntry>> result()
            {

                return !errors.Any()
                     ? Result<IEnumerable<domain.DiaryEntry>>.success(entries)
                     : Result<IEnumerable<domain.DiaryEntry>>.error(errors)
                     ;
            }

            private List<ResultError> errors = new List<ResultError>();
            private List<domain.DiaryEntry> entries = new List<domain.DiaryEntry>();
        }

        public static Result<domain.Day> create_day
                                          (IEnumerable<DataAttribute> attributes)
        {

            var errors = new List<ResultError>();
            var summary = string.Empty;
            time.Date date = null;
            var diary = Enumerable.Empty<domain.DiaryEntry>();

            var create_day = (Func<domain.Day>)(() => new domain.Day(
                 summary
                , date
                , diary
           ));

            attributes
             .value_for(
                 "summary"
                , error: () => new SummarySerialisationError()
             )
             .match(
                success: value => summary = value
               , error: errs => errors.AddRange(errs)
             )
             ;

            attributes
             .collection_for(
                  "date"
                 , error: () => new DateSerialisationError()
             )
             .bind(collection => time.serialisation.Date.create_date(collection))
             .match(
                  success: d => date = d
                 , error: errs => errors.AddRange(errs)
             );

            attributes
                      .collection_for(
                         "diary"
                        , error: () => new DiarySerialisationError()
                      )
                      // I. This does not handled failures in create_diary entry this has multiple otions and will need to be dealt with.
                      .bind(EEa =>



                       EEa
                        .Select((Ea, i) => {
                            return
                            DiaryEntry.create_diary_entry(



                                Ea
                            .match(s => default(IEnumerable<DataAttribute>),
                            ca => ca)



                          );




                        })
                        .Aggregate(
                           new AggregatedDiaryEntryResults()
                          , (acc, Re) => acc.add_result(Re)
                        )
                        .fmap(acc => acc.result())


                      )
                      .match(
                         success: diary_entries => diary = diary_entries
                        , error: errs => errors.AddRange(errs)
                       );

            /*

                    public static TSource Aggregate<TSource>( 
                          this IEnumerable<TSource> source
                         ,Func<TSource, TSource, TSource> func 
                    );
                    public static TAccumulate Aggregate<TSource, TAccumulate>( 
                         this IEnumerable<TSource> source
                        ,TAccumulate seed
                        ,Func<TAccumulate, TSource, TAccumulate> func 
                    );
                    public static TResult Aggregate<TSource, TAccumulate, TResult>( 
                         this IEnumerable<TSource> source
                        ,TAccumulate seed
                        ,Func<TAccumulate, TSource, TAccumulate> func
                        ,Func<TAccumulate, TResult> resultSelector 
                    );

            */

            return !errors.Any()
                 ? Result<domain.Day>.success(create_day())
                 : Result<domain.Day>.error(errors)
                 ;
        }

    }

    public static class DiaryEntry
    {

        public class StartTimeSerialisationError : ResultError { }


        public static IEnumerable<DataAttribute> create_memento
                                              (domain.DiaryEntry diary_entry)
        {

            return new DataAttribute[] {
                        DataAttribute.create_collection( "start_time", time.serialisation.ClockTime.create_memento( diary_entry.start_time )  )
                    };
        }

        public static Result<domain.DiaryEntry> create_diary_entry
                                                 (IEnumerable<DataAttribute> attributes)
        {

            var errors = new List<ResultError>();

            time.ClockTime start_time = null;


            attributes
             .collection_for(
                  "start_time"
                 , () => new StartTimeSerialisationError()
             )
             .bind(time.serialisation.ClockTime.create_clock_time)
             .match(
                 success: st => start_time = st
                , error: errs => errors.AddRange(errs)
             );

            return !errors.Any()
                 ? Result<domain.DiaryEntry>.success(new domain.DiaryEntry(start_time))
                 : Result<domain.DiaryEntry>.error(errors)
                 ;
        }
    }

}

namespace time
{


    public class Date
    {

        public int year { get; private set; }
        public int month { get; private set; }
        public int day { get; private set; }

        // Td. Implement value equality.

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

        public Date
                (DateTime time)
             : this(
                    year: time.Year
                   , month: time.Month
                   , day: time.Day
               )
        { }

    }

    public class Dates
    {

        public static Date today()
        {
            return new Date(DateTime.Now);
        }
    }

    // N. An interesting exercise would be in understaning what is sensible for Time respresentation.
    //     - This should include the timeline and ClockTimes.

    public class ClockTime
    {

        public int seconds_from_midnight { get; private set; }

        public ClockTime
                (int seconds_from_midnight)
        {

            if (!ClockTimes.SecondsAreWithinTwentyfourHourClock(seconds_from_midnight)) throw new Exception("Seconds from midnight is out of range.");

            this.seconds_from_midnight = seconds_from_midnight;
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


    namespace serialisation
    {

        public class Date
        {

            public class YearSerialisationError : ResultError { }

            public class MonthSerialisationError : ResultError { }

            public class DaySerialisationError : ResultError { }


            public class Memento
            {

                public int day { get; set; }

                public int month { get; set; }

                public int year { get; set; }

            }

            public static Memento create_memento
                                   (time.Date date)
            {

                return new Memento
                {
                    day = date.day
                    ,
                    month = date.month
                    ,
                    year = date.year
                };
            }

            public static time.Date create_date
                                     (Memento memento)
            {

                return new time.Date(
                     day: memento.day
                    , month: memento.month
                    , year: memento.year
                );
            }

            // Q. Why do return types not count as part of a method signatiure?
            public static IEnumerable<DataAttribute> create_attribute_memento
                                                                (time.Date date)
            {

                return new[] {
                     DataAttribute.create_value( "year", date.year.ToString() )
                    ,DataAttribute.create_value( "month", date.month.ToString() )
                    ,DataAttribute.create_value( "day", date.day.ToString() )
                };
            }


            public static Result<time.Date> create_date
                                             (IEnumerable<DataAttribute> attributes)
            {

                var errors = new List<ResultError>();
                var year = -1;
                var month = -1;
                var day = -1;

                var create_date = (Func<time.Date>)(() => new time.Date(
                    year: year
                   , month: month
                   , day: day
               ));

                attributes
                    .value_for(
                         "year"
                        , error: () => new YearSerialisationError()
                    )
                    .bind(ys => Results.ToInt(ys, () => new YearSerialisationError()))
                    .match(
                         success: yi => year = yi
                        , error: errs => errors.AddRange(errs)
                    )
                    ;

                attributes
                    .value_for(
                        "month"
                        , error: () => new MonthSerialisationError()
                    )
                    .bind(ms => Results.ToInt(ms, () => new MonthSerialisationError()))
                    .match(
                         success: mi => month = mi
                        , error: errs => errors.AddRange(errs)
                    );

                attributes
                    .value_for(
                         "day"
                        , error: () => new DaySerialisationError()
                    )
                    .bind(ds => Results.ToInt(ds, () => new DaySerialisationError()))
                    .match(
                         success: di => day = di
                        , error: errs => errors.AddRange(errs)
                    );

                return !errors.Any()
                     ? Result<time.Date>.success(create_date())
                     : Result<time.Date>.error(errors);
            }
        }

        public class ClockTime
        {

            public class SecondsFromMidnightSerialisationError : ResultError { }

            public static IEnumerable<DataAttribute> create_memento
                                                                (time.ClockTime clock_time)
            {

                return new DataAttribute[] {
                    DataAttribute.create_value( "seconds_from_midnight", clock_time.seconds_from_midnight.ToString() )
                };
            }


            public static Result<time.ClockTime> create_clock_time
                                                  (IEnumerable<DataAttribute> attributes)
            {

                var errors = new List<ResultError>();
                var seconds_from_midnight = -1;

                attributes
                 .value_for(
                     "seconds_from_midnight"
                    , () => new SecondsFromMidnightSerialisationError()
                 )
                 .bind(ms => Results.ToInt(ms, () => new SecondsFromMidnightSerialisationError()))
                 .match(
                     success: s => seconds_from_midnight = s
                    , error: errs => errors.AddRange(errs)
                 );

                return !errors.Any()
                     ? Result<time.ClockTime>.success(new time.ClockTime(seconds_from_midnight))
                     : Result<time.ClockTime>.error(errors)
                     ;
            }
        }

    }

}
