using System.Globalization;
using System.Reflection;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class FluentValidationSchemaTransformer(IServiceProvider serviceProvider)
    : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(context.JsonTypeInfo.Type);
        if (serviceProvider.GetService(validatorType) is not IValidator validator)
        {
            return Task.CompletedTask;
        }

        var descriptor = validator.CreateDescriptor();

        ExtractValidationRules(schema, context.JsonTypeInfo.Type, descriptor);

        return Task.CompletedTask;
    }

    private void ExtractValidationRules(
        OpenApiSchema schema,
        Type modelType,
        IValidatorDescriptor descriptor
    )
    {
        var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var propertyName = property.Name;
            var camelCasePropertyName = ToCamelCase(propertyName);

            if (
                schema.Properties is null
                || !schema.Properties.TryGetValue(
                    camelCasePropertyName,
                    out var propertySchemaInterface
                )
            )
            {
                continue;
            }

            if (propertySchemaInterface is not OpenApiSchema propertySchema)
            {
                continue;
            }

            var rules =
                descriptor.GetRulesForMember(propertyName)
                ?? descriptor.Rules.Where(rule => GetPropertyNameFromRule(rule) == propertyName);
            ApplyRulesToProperty(schema, propertySchema, camelCasePropertyName, rules);
        }
    }

    private static string? GetPropertyNameFromRule(IValidationRule rule)
    {
        try
        {
            var ruleType = rule.GetType();
            var propertyNameProperty = ruleType.GetProperty("PropertyName");
            return propertyNameProperty?.GetValue(rule) as string;
        }
        catch
        {
            return null;
        }
    }

    private void ApplyRulesToProperty(
        OpenApiSchema parentSchema,
        OpenApiSchema propertySchema,
        string propertyName,
        IEnumerable<IValidationRule> rules
    )
    {
        foreach (var rule in rules)
        {
            if (rule.Components is null)
            {
                continue;
            }

            foreach (var component in rule.Components)
            {
                ApplyValidationRule(
                    parentSchema,
                    propertySchema,
                    propertyName,
                    component.Validator
                );
            }
        }
    }

    private void ApplyValidationRule(
        OpenApiSchema parentSchema,
        OpenApiSchema propertySchema,
        string propertyName,
        IPropertyValidator validator
    )
    {
        switch (validator)
        {
            case INotNullValidator:
            case INotEmptyValidator:
                MakePropertyRequired(parentSchema, propertyName);
                break;

            case ILengthValidator lengthValidator:
                if (lengthValidator.Min > 0)
                    propertySchema.MinLength = lengthValidator.Min;
                if (lengthValidator.Max is > 0 and < int.MaxValue)
                    propertySchema.MaxLength = lengthValidator.Max;
                break;

            case IRegularExpressionValidator regexValidator:
                propertySchema.Pattern = regexValidator.Expression;
                break;

            case IBetweenValidator betweenValidator:
                if (IsNumeric(propertySchema.Type.ToString()))
                {
                    try
                    {
                        propertySchema.Minimum = Convert
                            .ToDecimal(betweenValidator.From)
                            .ToString(CultureInfo.InvariantCulture);
                        propertySchema.Maximum = Convert
                            .ToDecimal(betweenValidator.To)
                            .ToString(CultureInfo.InvariantCulture);

                        // Check if it's an exclusive between validator
                        var validatorTypeName = betweenValidator.GetType().Name;
                        var isExclusive = validatorTypeName.Contains(
                            "Exclusive",
                            StringComparison.OrdinalIgnoreCase
                        );
                        propertySchema.ExclusiveMinimum = $"{isExclusive}";
                        propertySchema.ExclusiveMaximum = $"{isExclusive}";
                    }
                    catch
                    {
                        // If conversion fails, skip this validation
                    }
                }
                break;

            case IComparisonValidator comparisonValidator:
                if (
                    IsNumeric(propertySchema.Type.ToString())
                    && comparisonValidator.ValueToCompare is not null
                )
                {
                    try
                    {
                        var value = Convert
                            .ToDecimal(comparisonValidator.ValueToCompare)
                            .ToString(CultureInfo.InvariantCulture);
                        switch (comparisonValidator.Comparison)
                        {
                            case Comparison.GreaterThan:
                                propertySchema.Minimum = value;
                                propertySchema.ExclusiveMinimum = "true";
                                break;
                            case Comparison.GreaterThanOrEqual:
                                propertySchema.Minimum = value;
                                propertySchema.ExclusiveMinimum = "false";
                                break;
                            case Comparison.LessThan:
                                propertySchema.Maximum = value;
                                propertySchema.ExclusiveMaximum = "true";
                                break;
                            case Comparison.LessThanOrEqual:
                                propertySchema.Maximum = value;
                                propertySchema.ExclusiveMaximum = "false";
                                break;
                            case Comparison.Equal:
                            case Comparison.NotEqual:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(
                                    nameof(comparisonValidator.Comparison),
                                    "Unsupported comparison type"
                                );
                        }
                    }
                    catch
                    {
                        // If conversion fails, skip this validation
                    }
                }
                break;

            case IEmailValidator:
                propertySchema.Format = "email";
                break;

            case ICreditCardValidator:
                propertySchema.Format = "credit-card";
                break;

            default:
                HandleOtherValidators(propertySchema, validator);
                break;
        }
    }

    private static void HandleOtherValidators(
        OpenApiSchema propertySchema,
        IPropertyValidator validator
    )
    {
        var validatorType = validator.GetType();
        var validatorName = validatorType.Name;

        if (validatorName.Contains("MinimumLength"))
        {
            var lengthProperty =
                validatorType.GetProperty("Min") ?? validatorType.GetProperty("Length");
            if (lengthProperty is null)
            {
                return;
            }

            var length = lengthProperty.GetValue(validator);
            if (length is int intLength)
            {
                propertySchema.MinLength = intLength;
            }
        }
        else if (validatorName.Contains("MaximumLength"))
        {
            var lengthProperty =
                validatorType.GetProperty("Max") ?? validatorType.GetProperty("Length");

            if (lengthProperty is null)
            {
                return;
            }

            var length = lengthProperty.GetValue(validator);
            if (length is int intLength)
            {
                propertySchema.MaxLength = intLength;
            }
        }
    }

    private static void MakePropertyRequired(OpenApiSchema schema, string propertyName)
    {
        schema.Required ??= new HashSet<string>();
        schema.Required.Add(propertyName);
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
        {
            return name;
        }
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static bool IsNumeric(string? schemaType)
    {
        return schemaType is "integer" or "number";
    }
}
