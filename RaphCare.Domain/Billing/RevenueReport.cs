using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Billing;

public class RevenueReport : BaseEntity
{
    public Guid ClinicId { get; set; }

    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public decimal ConsultationRevenue { get; set; }
    public decimal DeviceRevenue { get; set; }
}

