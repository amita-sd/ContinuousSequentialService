using System;
using System.Collections.Generic;
using System.Linq;

namespace Service
{

    /*

      Throwable and { vE } are different types because if you get a throwable you can not
      guarantee what state the environment is in so you can not trust any domain related
      information.

       sr -> sanatise         -> R . W . { vE } + Throwable
          -> validate request -> R . W . { vE } + Throwable
          -> validate context -> ( C . W )      + { vE } + Throwable
          -> apply -> W . W' . evnt             + { vE } + Throwable


       Qa. Is there a simple way of transitioning from the happy case to the error case ?
       QA. I do not think there is a generic way so each function requires its own specific transformation
           between the request and the command.  

           There may be a way to make the transformation from ValidationResult to R<CommandContext> generic
           see:  transform_validation_result_to_command_context

    */

    public class AttributeError
                  : ResultError
    {

        public string of { get; private set; }
        public string name { get; private set; }

        public IEnumerable<ResultError> errors { get; private set; }

        public AttributeError
                (string of
                , string name
                , IEnumerable<ResultError> errors)
        {

            this.of = of;
            this.name = name;
        }

    }


    public class AttributeCoundNotBeDeterminedInRequest : ResultError { }

    public class AttributeCoundNotBeConvertedToInt : ResultError { }

    public class AttributeCoundNotBeConvertedToString : ResultError { }

    public class AttributeCoundNotBeConvertedToStringCollection : ResultError { }

    public class AttributeCoundNotBeConvertedTo<T> : ResultError { }

    public class OutsideTwentyFourHourClockRange : ResultError { }


    public static class Addshift
    {

        /* 
           sr -> sanatise         -> R . W . { vE } + Throwable
              -> validate context -> ( C . W )      + { vE } + Throwable   
              -> apply            -> W . W' . evnt  + { vE } + Throwable
        */


        public static ServiceResponse apply
                            (ServiceRequest request)
        {
            var command_result =
            request
                .fmap(sanitise)
                .fmap(transform_validation_result_to_command_context)
                // I. add_shift should be a simple command and I should lift it in the bind function.  
                //     - The next step would then be to consider if I should have an equivalent to bind that just lift's the function into  a ->  Result b'
                .bind(add_shift)
                ;



            var ret =
            command_result.match(
            s => new ServiceResponse(s.world, s.new_world, Enumerable.Empty<DomainEvent>()),
            f => default(ServiceResponse)
            );

            return ret;
        }


        // Q. Complex data types from the attribute collection e.g. Date is created from three attributes?
        // Q. Lists from the attribute collection e.g. A request with a list of items?
        // Q. How do we deail with validation that is across sanitised fields? 
        private static ValidationResult sanitise
                                         (ServiceRequest request)
        {

            return new ValidationResult(
                request.employee_schedules,
                new Request(

                     title:
                        request
                         .attributes
                         .value_of("NewShiftRequest", "title", () => new AttributeCoundNotBeDeterminedInRequest())
                         .bind(val => Results.ToString(val, () => new AttributeCoundNotBeConvertedToString()))

                //, duration_in_seconds:
                //    request
                //     .attributes
                //     .value_of("NewShiftRequest", "duration_in_seconds", () => new AttributeCoundNotBeDeterminedInRequest())
                //     // Q. Can I add a convert method that has the same signature as verify? 
                //     .bind(duration => Results.ToInt(duration, () => new AttributeCoundNotBeConvertedToInt()))
                //     .verify(ClockTimes.SecondsAreWithinTwentyfourHourClock, () => new OutsideTwentyFourHourClockRange())

                //, employee_names:
                //    request
                //    .attributes
                //    .value_of("NewShiftRequest", "employee_names", () => new AttributeCoundNotBeDeterminedInRequest())
                //    .bind(v => Results.ToStringCollection(v, () => new AttributeCoundNotBeConvertedToStringCollection()))

                //, date:
                //    request
                //    .attributes
                //    .value_of("NewShiftRequest", "dates", () => new AttributeCoundNotBeDeterminedInRequest())
                //    .bind(v => Results.ToDate(v, () => new AttributeCoundNotBeConvertedTo<Date>()))


                ),
                new ResultError[] { }
            );


        }


        private static Result<CommandContext> transform_validation_result_to_command_context
                                               (ValidationResult validation_result)
        {

            // Q. Is Func<EmployeeSchedules> going to provide any benefit?
            // Q. Should we add an Extension mehthod that converts a type to a result? 
            return validation_result
                    .request
                    .fmap(convert_request_to_command)
                    .bind(c => Result<CommandContext>.success(new CommandContext(validation_result.employee_schedules, c)))
                    .match(
                        success: c =>
                            validation_result.errors.Any()
                                ? Result<CommandContext>.error(validation_result.errors)
                                : Result<CommandContext>.success(c),

                        error: errs =>
                            Result<CommandContext>.error(errs.Union(validation_result.errors))
                    );

        }

        private static Result<Command> convert_request_to_command
                                        (Request request)
        {

            var errors = new List<ResultError>();

            var title = string.Empty;
            //var duration_in_seconds = -1;

            request
                .title
                .match(
                    success: t => title = t,
                    error: errs => errors.Add(new AttributeError("AddShiftCommand", "title", errs))
                 );

            //request
            //    .duration_in_seconds
            //    .match(
            //        success: d => duration_in_seconds = d,
            //        error: errs => errors.Add(new AttributeError("AddShiftCommand", "duration_in_seconds", errs))
            //    );


            return !errors.Any()
                 ? Result<Command>.success(new Command(title))
                 : Result<Command>.error(errors)
                 ;
        }


        private static Result<CommandResult> add_shift
                                              (CommandContext context)
        {


            var new_count = context.employee_schedules().count + 1;

            return
                Result<CommandResult>
                .success(
                    new CommandResult(context.employee_schedules
                                        , () => new EmployeeSchedules(new_count)
                                    )
                        );

        }

        private class ValidationResult
        {

            public Func<EmployeeSchedules> employee_schedules { get; private set; }

            public Request request { get; private set; }

            public IEnumerable<ResultError> errors { get; private set; }

            public ValidationResult
                    (Func<EmployeeSchedules> employee_schedules
                    , Request request
                    , IEnumerable<ResultError> errors)
            {

                this.employee_schedules = employee_schedules;
                this.request = request;
                this.errors = errors;

            }

        }

        private class Request
        {

            public Result<string> title { get; private set; }

            //public Result<int> duration_in_seconds { get; private set; }

            //public Result<IEnumerable<string>> employee_names { get; private set; }

            //public Result<Date> date { get; private set; }


            public Request
                    (Result<string> title
                     //, Result<int> duration_in_seconds
                     //, Result<IEnumerable<string>> employee_names
                     //, Result<Date> date
                     )
            {
                this.title = title;
                //this.duration_in_seconds = duration_in_seconds;
                //this.employee_names = employee_names;
                //this.date = date;
            }

        }

        private class Command
        {

            public string title { get; private set; }

            //public int duration_in_seconds { get; private set; }

            public Command
                    (string title
                    //, int duration_in_seconds
                    )
            {

                this.title = title;
                //this.duration_in_seconds = duration_in_seconds;
            }
        }

        private class CommandContext
        {

            public Func<EmployeeSchedules> employee_schedules { get; private set; }

            public Command command { get; private set; }

            public CommandContext
                    (Func<EmployeeSchedules> employee_schedules
                    , Command command)
            {

                this.employee_schedules = employee_schedules;
                this.command = command;
            }
        }

        private class CommandResult
        {

            public Func<EmployeeSchedules> world { get; private set; }

            public Func<EmployeeSchedules> new_world { get; private set; }


            public CommandResult
                    (Func<EmployeeSchedules> world
                    , Func<EmployeeSchedules> new_world)
            {

                this.world = world;
                this.new_world = new_world;
            }

        }

        /*  General

            Throwable and { vE } are different types because if you get a throwable you can not
            guarantee what state the environment is in so you can not trust any domain related
            information.

             sr -> sanatise         -> R . W . { vE } + Throwable
                -> validate request -> R . W . { vE } + Throwable
                -> validate context -> ( C . W )      + { vE } + Throwable   
                -> apply -> W . W' . evnt             + { vE } + Throwable


            Shift request attributes
                title        
                colour      
                start date
                start time hours
                start time minutes
                duration hours
                duration seconds


            Shift request
                title  - 
                    string
                    Mandatory 
                    ! whitespace
                    < 100 
                    not empty

                colour - 
                    if not specified use default
                    is valid RGB

                start time in seconds from midnight -
                    int 
                    Mandatory
                    <= 24 hours

                duration in seconds -
                    int
                    <= 24 hours
                                    
        */
    }



}
