using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SportAcademy.Domain.Entities.Tenants;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public bool IsActive { get; set; }

    public ICollection<TenantSubscription> Subscriptions { get; set; } = [];
    public ICollection<SubscriptionPlanFeature> Features { get; set; } = [];

}
