using System;
using System.Collections.Generic;
using System.Data;

namespace eMarketing.AdminPanel.Models
{
    public sealed class ReportDto
    {
        public string ReportId { get; set; }
        public string ReportType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StoreName { get; set; }
        public string CreatedBy { get; set; }
        public string SummaryHash { get; set; }
        public List<ReportMetricDto> Metrics { get; private set; }
        public List<ReportTableDto> Tables { get; private set; }

        public ReportDto()
        {
            Metrics = new List<ReportMetricDto>();
            Tables = new List<ReportTableDto>();
        }
    }

    public sealed class ReportMetricDto
    {
        public string Title { get; set; }
        public string Value { get; set; }
    }

    public sealed class ReportTableDto
    {
        public string Title { get; set; }
        public DataTable Data { get; set; }
        public string[] Columns { get; set; }
    }
}
