using System.Collections.Generic;

namespace Core.ReportGeneration
{
    public interface IReportable
    {
        IEnumerable<ReportSection> GetReportSections();
    }
}
