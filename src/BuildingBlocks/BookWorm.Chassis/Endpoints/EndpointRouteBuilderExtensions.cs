using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Chassis.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder ProducesPost<T>(bool hasValidation = true)
        {
            builder = builder.Produces<T>(StatusCodes.Status201Created);

            if (hasValidation)
            {
                builder = builder.ProducesValidationProblem();
            }

            return builder;
        }

        public RouteHandlerBuilder ProducesPostWithoutLocation<T>(bool hasValidation = true)
        {
            builder = builder.Produces<T>();

            if (hasValidation)
            {
                builder = builder.ProducesValidationProblem();
            }

            return builder;
        }

        public RouteHandlerBuilder ProducesPut()
        {
            return builder
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem();
        }

        public RouteHandlerBuilder ProducesDelete()
        {
            return builder
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        public RouteHandlerBuilder ProducesGet<T>(
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

        public RouteHandlerBuilder ProducesPatch<T>(bool hasValidation = true)
        {
            builder = builder.Produces<T>().ProducesProblem(StatusCodes.Status404NotFound);

            if (hasValidation)
            {
                builder = builder.ProducesValidationProblem();
            }

            return builder;
        }
    }
}
