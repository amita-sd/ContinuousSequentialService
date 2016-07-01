using System;
using System.Collections.Generic;
using AmitaAttribute = amita.primitives.net.Attribute;

namespace Domain
{

    public class ServiceRequest
    {

        public IEnumerable<AmitaAttribute> attributes { get; private set; }

        public Func<EmployeeScheduleStartDate> employee_schedules { get; private set; }

        public IEnumerable<Permission> user_permissions { get; private set; }


        public ServiceRequest
                (IEnumerable<AmitaAttribute> attributes
                , Func<EmployeeScheduleStartDate> employee_schedules
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

        public Func<EmployeeScheduleStartDate> original_employee_schedules { get; private set; }

        public Func<EmployeeScheduleStartDate> adjusted_employee_schedules { get; private set; }

        public IEnumerable<DomainEvent> domain_events { get; private set; }


        public ServiceResponse
                (Func<EmployeeScheduleStartDate> original_employee_schedules
                , Func<EmployeeScheduleStartDate> adjusted_employee_schedules
                , IEnumerable<DomainEvent> domain_events)
        {

            this.original_employee_schedules = original_employee_schedules;
            this.adjusted_employee_schedules = adjusted_employee_schedules;
            this.domain_events = domain_events;
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


}
