using System.ComponentModel;

namespace BookWorm.Chassis.Specification;

public enum OrderType : byte
{
    [Description("Order By")]
    OrderBy = 0,

    [Description("Order By Descending")]
    OrderByDescending = 1,

    [Description("Then By")]
    ThenBy = 2,

    [Description("Then By Descending")]
    ThenByDescending = 3,
}
