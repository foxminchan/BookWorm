using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Chassis.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteHandlerBuilder ProducesPost<T>(
        this RouteHandlerBuilder builder,
        bool hasValidation = true
    )
    {
        builder = builder.Produces<T>(StatusCodes.Status201Created);

        if (hasValidation)
        {
            builder = builder.ProducesValidationProblem();
        }

        return builder;
    }

    public static RouteHandlerBuilder ProducesPostWithoutLocation<T>(
        this RouteHandlerBuilder builder,
        bool hasValidation = true
    )
    {
        builder = builder.Produces<T>();

        if (hasValidation)
        {
            builder = builder.ProducesValidationProblem();
        }

        return builder;
    }

    public static RouteHandlerBuilder ProducesPut(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
    }

    public static RouteHandlerBuilder ProducesDelete(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static RouteHandlerBuilder ProducesGet<T>(
        this RouteHandlerBuilder builder,
        bool hasValidation = false,
        bool hasNotFound = false
    )
    {
        builder = builder.Produces<T>();

        if (hasValidation)
        {
            builder = builder.ProducesValidationProblem();
        }

        if (hasNotFound)
        {
            builder = builder.ProducesProblem(StatusCodes.Status404NotFound);
        }

        return builder;
    }

    public static RouteHandlerBuilder ProducesPatch<T>(
        this RouteHandlerBuilder builder,
        bool hasValidation = true
    )
    {
        builder = builder.Produces<T>().ProducesProblem(StatusCodes.Status404NotFound);

        if (hasValidation)
        {
            builder = builder.ProducesValidationProblem();
        }

        return builder;
    }
}
