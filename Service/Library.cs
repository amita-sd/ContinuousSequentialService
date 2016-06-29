using System;
using System.Collections.Generic;
using System.Linq;

namespace Service
{

    public class ServiceRequest
    {

        public IEnumerable<RequestAttribute> attributes { get; private set; }

        public Func<EmployeeSchedules> employee_schedules { get; private set; }

        public IEnumerable<Permission> user_permissions { get; private set; }


        public ServiceRequest
                (IEnumerable<RequestAttribute> attributes
                , Func<EmployeeSchedules> employee_schedules
                , IEnumerable<Permission> user_permissions)
        {

            this.attributes = attributes;
            this.employee_schedules = employee_schedules;
            this.user_permissions = user_permissions;
        }

    }


    // Q. What shape should the ServiceResponse be?
    public class ServiceResponse
    {

        public Func<EmployeeSchedules> original_employee_schedules { get; private set; }

        public Func<EmployeeSchedules> adjusted_employee_schedules { get; private set; }

        public IEnumerable<DomainEvent> domain_events { get; private set; }


        public ServiceResponse
                (Func<EmployeeSchedules> original_employee_schedules
                , Func<EmployeeSchedules> adjusted_employee_schedules
                , IEnumerable<DomainEvent> domain_events)
        {

            this.original_employee_schedules = original_employee_schedules;
            this.adjusted_employee_schedules = adjusted_employee_schedules;
            this.domain_events = domain_events;
        }
    }

    public static class Helpers
    {

        public static Q fmap<P, Q>
                         (this P p
                         , Func<P, Q> f)
        {

            return f(p);
        }

        public static P @do<P>
                         (this P p
                         , Action<P> a)
        {

            a(p);
            return p;
        }
    }

    public class RequestAttribute
    {

        public string of { get; private set; }

        public string name { get; private set; }

        public IAttributeValue value { get; private set; }

        public RequestAttribute
        (string of
                 , string name
                 , IAttributeValue value)
        {

            this.of = of;
            this.name = name;
            this.value = value;
        }
    }

    //public class Attribute
    //{

    //    public string of { get; private set; }

    //    public string name { get; private set; }

    //    public string value { get; private set; }

    //    public Attribute
    //            (string of
    //            , string name
    //             , string value)
    //    {

    //        this.of = of;
    //        this.name = name;
    //        this.value = value;
    //    }
    //}

    public interface IAttributeValue
    {

    }

    public static class AttributeValuePatternMatcher
    {
        public static T Match<T>(this IAttributeValue source
                                    , Func<ASimpleValue, T> is_simple_value
                                    , Func<ACollectionValue, T> is_collection_value
                                    , Func<AComplexValue, T> is_complex_value)
        {

            if (source is ASimpleValue)
            {
                return is_simple_value(source as ASimpleValue);
            }

            if (source is ACollectionValue)
            {
                return is_collection_value(source as ACollectionValue);
            }

            if (source is AComplexValue)
            {
                return is_complex_value(source as AComplexValue);
            }

            throw new Exception("Type could not be determined");

        }


        public static string AsString(this IAttributeValue source)
        {
            return (source as ASimpleValue).value;
        }

        public static int AsInt(this IAttributeValue source)
        {
            return int.Parse((source as ASimpleValue).value);
        }
    }



    public class ASimpleValue : IAttributeValue
    {
        public string value { get; private set; }

        public ASimpleValue(string value)
        {
            this.value = value;
        }
    }

    public class ACollectionValue : IAttributeValue
    {

        public IEnumerable<IAttributeValue> value { get; private set; }

        public ACollectionValue(params IAttributeValue[] value)
        {
            this.value = value;
        }

        public ACollectionValue(IEnumerable<IAttributeValue> value)
        {
            this.value = value;
        }
    }

    public class AComplexValue : IAttributeValue
    {
        public Dictionary<string, IAttributeValue> value { get; private set; }

        public AComplexValue(Dictionary<string, IAttributeValue> value)
        {
            this.value = value;
        }
    }

    public static class AttributeExtensions
    {

        public static Result<IAttributeValue> value_of
                                      (this IEnumerable<RequestAttribute> attributes
                                      , string of
                                      , string name
                                      , Func<ResultError> no_matches_error_message
                                      , Func<ResultError> multiple_matches_error_message)
        {

            // ###
            //   # -> []              error
            //   # -> [ {}, ..., {} ] error 
            //   # -> [ {} ]          happy
            var matches = attributes
                            .Where(a => a.of == of && a.name == name)
                            .ToArray()
                            ;

            if (!attributes.Any())
            {
                return Result<IAttributeValue>.error(no_matches_error_message());

            }
            else if (attributes.Count() > 1)
            {
                return Result<IAttributeValue>.error(multiple_matches_error_message());

            }
            else
            {
                // I. This is not completely sound as the attribute value could be null.

                return Result<IAttributeValue>.success(attributes.First().value);
            }
        }

        public static Result<IAttributeValue> value_of
                                      (this IEnumerable<RequestAttribute> attributes
                                      , string of
                                      , string name
                                      , Func<ResultError> error)
        {

            return value_of(
                attributes,
                of,
                name,
                error,
                error
            );
        }


    }

    public abstract class Permission { }
    public sealed class CanStandEmployeeDown : Permission { }
    public sealed class CanCallEmployeeIn : Permission { }
    public sealed class CanAddTacticalShift : Permission { }

    internal static class Permissions
    {

        public static IEnumerable<Permission> GetPermissions()
        {

            var random = new Random();
            var cardinality = random.Next(1, all_permisions.Length);

            for (var i = 0; i < cardinality; i++)
            {
                yield return all_permisions[random.Next(0, all_permisions.Length)];
            }
        }


        private static Permission[] all_permisions = new Permission[] {
            new CanStandEmployeeDown(),
            new CanCallEmployeeIn(),
            new CanAddTacticalShift(),
        };

    }

    public abstract class DomainEvent { }

    public abstract class StateChangeEvent { }

    public abstract class ResultError { }

    /// <summary>
    /// Standard result monad that can either be a success or an error.  If it is
    /// an error then there will a a list of one or more errors describing what the
    /// problem is.
    /// </summary>
    public class Result<Q>
    {

        // Create a success result with argument as the value.  There is a 
        // check on the Succees result constructor to make sure that the
        // value actually has one, if this is null then an exception will
        // be thrown.
        public static Result<Q> success
                                 (Q value)
        {

            return new Success(value);

        }

        // Create an error result with the supplied errors.  There is a
        // check on the Error result constructor to make sure that the errors
        // have been supplied.
        public static Result<Q> error
                                 (IEnumerable<ResultError> errors)
        {

            return new Error(errors);
        }

        public static Result<Q> error
                                 (ResultError error)
        {

            return new Error(new[] { error });
        }


        // Calls the transformation function if it is a success otherwise
        // converts the errors to the new error type.
        public Result<P> bind<P>
                          (Func<Q, Result<P>> f)
        {

            return match(
                success: f,
                error: errors => new Result<P>.Error((this as Error).errors)
            );
        }

        public Result<Q> verify
                          (Func<Q, bool> f
                          , Func<ResultError> e)
        {

            return bind(val => f(val) ? this : error(e()));

        }

        // call the apprpriate action base on the result status.
        public void match
                    (Action<Q> success
                    , Action<IEnumerable<ResultError>> error)
        {


            this
              .match(
                success: value => { success(value); return new object(); },
                error: errors => { error(errors); return new object(); }
              );

        }

        // Calls and returns the result of the appropriate func
        // based on the result status.
        public P match<P>
                  (Func<Q, P> success
                  , Func<IEnumerable<ResultError>, P> error)
        {

            if (this is Success)
            {
                return success((this as Success).value);
            }

            if (this is Error)
            {
                return error((this as Error).errors);
            }

            throw new Exception("Unexpected case");

        }


        // Ensure that the class can not be inherited or directly
        // constructed.
        private Result() { }


        private class Success
                       : Result<Q>
        {

            public Q value { get; private set; }

            public Success
                    (Q value)
            {

                if (value == null) throw new Exception();

                this.value = value;
            }

        }

        private class Error
                       : Result<Q>
        {

            public IEnumerable<ResultError> errors { get; private set; }

            public Error
                    (IEnumerable<ResultError> errors)
            {

                if (errors == null) throw new Exception("Errors was null!");

                this.errors = errors;
            }

        }

    }

    public static class Results
    {

        public static Result<int> ToInt
                                   (this string value
                                   , Func<ResultError> error)
        {

            int as_int;

            return int.TryParse(value, out as_int)
                 ? Result<int>.success(as_int)
                 : Result<int>.error(error())
                 ;
        }

        public static Result<int> ToInt
                                    (this IAttributeValue value
                                    , Func<ResultError> error)
        {
            return
                value
                    .Match(
                        simple => {

                            int as_int;

                            return
                                int.TryParse(simple.value, out as_int) ?
                                   Result<int>.success(as_int) :
                                   Result<int>.error(error())
                               ;
                        },
                        collection => Result<int>.error(error()),
                        complex => Result<int>.error(error())
                    );
        }

        public static Result<string> ToString
                                    (this IAttributeValue value
                                     , Func<ResultError> error)
        {

            return
                value
                    .Match(
                        simple => Result<string>.success(simple.value),
                        collection => Result<string>.error(error()),
                        complex => Result<string>.error(error())
                    );
        }

        public static Result<IEnumerable<string>> ToStringCollection
                                    (IAttributeValue value
                                     , Func<ResultError> error)
        {

            return
                value
                    .Match(
                        simple => Result<IEnumerable<string>>.error(error()),
                        collection => {
                            try
                            {
                                return
                                    Result<IEnumerable<string>>.success(
                                        collection.value.Select(i => i.AsString())
                                    );
                            }
                            catch (Exception e)
                            {
                                return Result<IEnumerable<string>>.error(error());
                            }
                        },
                        complex => Result<IEnumerable<string>>.error(error())
                    );
        }

        public static Result<Date> ToDate
                                    (IAttributeValue value
                                     , Func<ResultError> error)
        {

            return
                value
                    .Match(
                        simple => Result<Date>.error(error()),
                        collection => Result<Date>.error(error()),
                        complex => {
                            try
                            {
                                return
                                    Result<Date>.success(

                                        new Date(complex.value["year"].AsInt()
                                                  , complex.value["month"].AsInt()
                                                  , complex.value["day"].AsInt())

                                    );
                            }
                            catch (Exception e)
                            {
                                return Result<Date>.error(error());
                            }
                        }

                    );
        }

    }

}
