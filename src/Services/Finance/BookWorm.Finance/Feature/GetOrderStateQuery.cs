using BookWorm.Chassis.Query;
using MassTransit.SagaStateMachine;
using MassTransit.Visualizer;

namespace BookWorm.Finance.Feature;

public sealed record GetOrderStateQuery : IQuery<string>;

public sealed class GetOrderStateHandler(ILoggerFactory loggerFactory)
    : IQueryHandler<GetOrderStateQuery, string>
{
    public Task<string> Handle(GetOrderStateQuery request, CancellationToken cancellationToken)
    {
        var orderStateMachine = new OrderStateMachine(loggerFactory);
        var graph = orderStateMachine.GetGraph();
        var generator = new StateMachineGraphvizGenerator(graph);
        var dots = generator.CreateDotFile();
        return Task.FromResult(dots);
    }
}
