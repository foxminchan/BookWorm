using System.ComponentModel;

namespace BookWorm.Chassis.Specification;

public enum IncludeType : byte
{
    [Description("Include")]
    Include = 0,

    [Description("Then Include")]
    ThenInclude = 1,
}
