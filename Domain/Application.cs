namespace Domain
{
    public class EmployeeScheduleStartDate
    {
        public domain.Day start_date { get; private set; }

        public EmployeeScheduleStartDate(domain.Day the_start_date)
        {
            this.start_date = the_start_date;
        }

        public override string ToString()
        {
            return $"Summary: {start_date.summary},\n Date: {start_date.date.day}/{start_date.date.month}/{start_date.date.year}";
        }
    }
}
