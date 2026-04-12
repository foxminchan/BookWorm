using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Chassis.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        /// <summary>
        ///     Configures response metadata for a POST endpoint that returns a created resource.
        /// </summary>
        /// <typeparam name="T">
        ///     The response payload type.
        /// </typeparam>
        /// <param name="hasValidation">
        ///     <see langword="true" /> to include validation problem metadata; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     The configured route handler builder.
        /// </returns>
        public RouteHandlerBuilder ProducesPost<T>(bool hasValidation = true)
        {
            builder = builder.Produces<T>(StatusCodes.Status201Created);

            if (hasValidation)
            {
                builder = builder.ProducesValidationProblem();
            }

            return builder;
        }

        /// <summary>
        ///     Configures response metadata for a POST endpoint that returns a success payload without a location header.
        /// </summary>
        /// <typeparam name="T">
        ///     The response payload type.
        /// </typeparam>
        /// <param name="hasValidation">
        ///     <see langword="true" /> to include validation problem metadata; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     The configured route handler builder.
        /// </returns>
        public RouteHandlerBuilder ProducesPostWithoutLocation<T>(bool hasValidation = true)
        {
            builder = builder.Produces<T>();

            if (hasValidation)
            {
                builder = builder.ProducesValidationProblem();
            }

            return builder;
        }

        /// <summary>
        ///     Configures response metadata for a PUT endpoint.
        /// </summary>
        /// <returns>
        ///     The configured route handler builder.
        /// </returns>
        public RouteHandlerBuilder ProducesPut()
        {
            return builder
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem();
        }

        /// <summary>
        ///     Configures response metadata for a DELETE endpoint.
        /// </summary>
        /// <returns>
        ///     The configured route handler builder.
        /// </returns>
        public RouteHandlerBuilder ProducesDelete()
        {
            return builder
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        /// <summary>
        ///     Configures response metadata for a GET endpoint.
        /// </summary>
        /// <typeparam name="T">
        ///     The response payload type.
        /// </typeparam>
        /// <param name="hasValidation">
        ///     <see langword="true" /> to include validation problem metadata; otherwise, <see langword="false" />.
        /// </param>
        /// <param name="hasNotFound">
        ///     <see langword="true" /> to include not found problem metadata; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     The configured route handler builder.
        /// </returns>
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

        /// <summary>
        ///     Configures response metadata for a PATCH endpoint.
        /// </summary>
        /// <typeparam name="T">
        ///     The response payload type.
        /// </typeparam>
        /// <param name="hasValidation">
        ///     <see langword="true" /> to include validation problem metadata; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     The configured route handler builder.
        /// </returns>
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
