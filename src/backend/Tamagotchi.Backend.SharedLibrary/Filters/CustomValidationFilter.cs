using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
using Tamagotchi.Backend.SharedLibrary.Attributes;
using Tamagotchi.Backend.SharedLibrary.Models;

namespace Tamagotchi.Backend.SharedLibrary.Filters;

public class CustomValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var errors = new List<ValidationError>();

        //Check if the email exists in the query string
        if (context.HttpContext.Request.Query.TryGetValue("email", out var emailValues))
        {
            var email = emailValues.ToString();

            if (!IsValidEmail(email))
            {
                errors.Add(new ValidationError
                {
                    Field = "email",
                    Error = "Invalid email format."
                });
            }
        }

        // Check if ModelState is invalid
        if (!context.ModelState.IsValid)
        {
            errors.AddRange(context.ModelState
                .Where(ms => ms.Value != null && ms.Value.Errors.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => new ValidationError
                {
                    Field = ms.Key,
                    Error = e.ErrorMessage,
                })));
        }

        // If there are errors, return a BadRequest response
        if (errors.Any())
        {
            var resourceIdentifier = context
                .ActionDescriptor.FilterDescriptors.Select(fd => fd.Filter)
                .OfType<ResourceFilterIdentifier>()
                .FirstOrDefault()
                ?.ResourceIdentifier;

            if (resourceIdentifier == null)
            {
                resourceIdentifier = context
                    .ActionDescriptor.EndpointMetadata.OfType<ResourceFilterIdentifier>()
                    .FirstOrDefault()
                    ?.ResourceIdentifier;
            }

            context.Result = new BadRequestObjectResult(
                ApiFailureResponse.ValidationFailureResponse(
                    "One or more validation errors occurred.",
                    errors,
                    $"EC-BPL-400"
                )
            );
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No post-processing required
    }

    private bool IsValidEmail(string email)
    {
        var emailAttribute = new EmailAddressAttribute();
        return emailAttribute.IsValid(email);
    }
}
