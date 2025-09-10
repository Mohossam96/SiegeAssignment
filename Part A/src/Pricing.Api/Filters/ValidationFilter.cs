using MiniValidation;

namespace Pricing.Api.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.GetArgument<T>(0);
        if (!MiniValidator.TryValidate(argument, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}