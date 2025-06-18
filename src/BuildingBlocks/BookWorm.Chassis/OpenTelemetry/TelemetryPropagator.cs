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

    public static PropagationContext Extract<T>(
        PropagationContext context,
        T carrier,
        Func<T, string, IEnumerable<string>> getter
    )
    {
        _propagator.Extract(context, carrier, getter);

        return context;
    }

    public static PropagationContext Extract<T>(
        T carrier,
        Func<T, string, IEnumerable<string>> getter
    )
    {
        return _propagator.Extract(default, carrier, getter);
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

    public static PropagationContext? GetPropagationContext(Activity? activity = null)
    {
        var activityContext = (activity ?? Activity.Current)?.Context;

        if (!activityContext.HasValue)
        {
            return null;
        }

        return new PropagationContext(activityContext.Value, Baggage.Current);
    }
}
