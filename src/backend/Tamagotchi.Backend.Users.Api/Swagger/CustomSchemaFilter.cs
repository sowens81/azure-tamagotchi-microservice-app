using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.Users.Api.Dtos;

namespace Tamagotchi.Backend.Users.Api.Swagger;

public class CustomSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(ApiSuccessResponse<IEnumerable<UserResponseDto>>))
        {
            schema.Title = "ApiSuccessResponseUsers";
        }

        if (context.Type == typeof(ApiSuccessResponse<UserResponseDto>))
        {
            schema.Title = "ApiSuccessResponseUser";
        }

        if (context.Type == typeof(ApiSuccessResponse<UserUpdateResponseDto>))
        {
            schema.Title = "ApiSuccessResponseUpdatedUser";
        }

        if (context.Type == typeof(ApiSuccessResponse<UserRegistrationResponseDto>))
        {
            schema.Title = "ApiSuccessResponseRegisteredUser";
        }

        if (context.Type == typeof(UserRegistrationResponseDto))
        {
            schema.Title = "UserRegistrationResponse";
        }

        if (context.Type == typeof(UserRegistrationRequestDto))
        {
            schema.Title = "UserRegistrationRequest";
        }

        if (context.Type == typeof(UserUpdateResponseDto))
        {
            schema.Title = "UserUpdateResponse";
        }

        if (context.Type == typeof(UserUpdateRequestDto))
        {
            schema.Title = "UserUpdateRequest";
        }

        if (context.Type == typeof(ApiSuccessResponse<AuthenticationResponseDto>))
        {
            schema.Title = "ApiSuccessAuthenticatedUser";
        }

        if (context.Type == typeof(AuthenticationResponseDto))
        {
            schema.Title = "AuthenticationResponse";
        }

        if (context.Type == typeof(AuthenticationRequestDto))
        {
            schema.Title = "AuthenticationRequest";
        }

        if (context.Type == typeof(UserResponseDto))
        {
            schema.Title = "UserResponse";
        }
    }
}
