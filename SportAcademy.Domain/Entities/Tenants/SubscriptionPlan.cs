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

    // Public-marketing-site presentation - deliberately separate from IsActive (which governs
    // whether a *tenant* can be placed on this plan). A plan can stay active for existing
    // tenants while being pulled from the public pricing page (IsPubliclyListed = false), e.g.
    // a legacy plan nobody sells any more but current subscribers keep.
    public bool IsPubliclyListed { get; set; } = true;
    public int DisplayOrder { get; set; }
    public bool IsHighlighted { get; set; }

    public ICollection<TenantSubscription> Subscriptions { get; set; } = [];
    public ICollection<SubscriptionPlanFeature> Features { get; set; } = [];

}
