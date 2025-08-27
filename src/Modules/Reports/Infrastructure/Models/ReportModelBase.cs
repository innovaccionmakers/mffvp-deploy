namespace Reports.Infrastructure.Models
{
    public abstract class ReportModelBase
    {
        public abstract object[] ToRowData();
    }
}
