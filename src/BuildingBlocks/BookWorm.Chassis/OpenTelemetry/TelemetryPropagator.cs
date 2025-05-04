using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace BookWorm.Chassis.OpenTelemetry;

public static class TelemetryPropagator
{
    private static readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;

    public static void Inject<T>(
        this PropagationContext context,
        T carrier,
        Action<T, string, string> setter
    )
    {
        _propagator.Inject(context, carrier, setter);
    }

    public static PropagationContext? Propagate<T>(
        this Activity? activity,
        T carrier,
        Action<T, string, string> setter
    )
    {
        if (activity?.Context is null)
        {
            return null;
        }

        var propagationContext = new PropagationContext(activity.Context, Baggage.Current);
        propagationContext.Inject(carrier, setter);

        return propagationContext;
    }
}
