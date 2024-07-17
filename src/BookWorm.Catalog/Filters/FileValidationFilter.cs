﻿using FluentValidation;
using FluentValidation.Results;

namespace BookWorm.Catalog.Filters;

public sealed class FileValidationFilter : IEndpointFilter
{
    private const int MaxFileSize = 1048576;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        var formCollection = await request.ReadFormAsync();
        var files = formCollection.Files;

        if (files.Count == 0)
            return await next(context);

        List<ValidationFailure> errors = [];

        foreach (var file in files)
        {
            switch (file.Length)
            {
                case 0:
                    errors.Add(new("Length", "File is empty"));
                    break;
                case > MaxFileSize:
                    errors.Add(new("Length", $"File size is too large. Max file size is {MaxFileSize / 1024} KB"));
                    break;
            }

            List<string> allowedContentTypes = ["image/jpeg", "image/png", "image/jpg"];

            if (allowedContentTypes.Contains(file.ContentType)) continue;

            errors.Add(
                new("ContentType",
                    $"File type is not allowed. Allowed file types are {string.Join(", ", allowedContentTypes)}"));
        }

        if (errors.Count > 0)
            throw new ValidationException(errors.AsEnumerable());

        return await next(context);
    }
}