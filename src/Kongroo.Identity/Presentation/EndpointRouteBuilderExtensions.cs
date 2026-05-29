using System.ComponentModel;
using System.Security.Claims;
using Kongroo.BuildingBlocks.Presentation.Authorization;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Kongroo.Identity.Presentation;

public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteGroupBuilder MapIdentityEndpoints()
        {
            var routeGroup = endpoints.MapGroup("/identity").WithTags("Identity");
            var usersGroup = routeGroup.MapGroup("/users");

            usersGroup
                .MapPost("/", CreateUserAsync)
                .AllowAnonymous()
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithName("CreateUser")
                .WithSummary("Register a user account")
                .WithDescription(
                    "Creates a user account and returns the public profile information for the new identity."
                );

            usersGroup
                .MapGet("/", GetUsersAsync)
                .RequireAuthorization(AuthorizationPolicies.AdminOnly)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithName("GetUsers")
                .WithSummary("Get users")
                .WithDescription("Returns all user accounts ordered by username for administrative management.");

            usersGroup
                .MapGet("/me", GetCurrentUserAsync)
                .RequireAuthorization()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithName("GetCurrentUser")
                .WithSummary("Get the authenticated user account")
                .WithDescription("Returns the public profile information for the authenticated user.");

            usersGroup
                .MapGet("/{userId:guid}", GetUserAsync)
                .RequireAuthorization(AuthorizationPolicies.AdminOnly)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithName("GetUserById")
                .WithSummary("Get a user account")
                .WithDescription("Returns the administrative management view for an existing user account.");

            usersGroup
                .MapPut("/{userId:guid}/role", UpdateUserRoleAsync)
                .RequireAuthorization(AuthorizationPolicies.AdminOnly)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithName("UpdateUserRole")
                .WithSummary("Update a user role")
                .WithDescription("Sets the role of an existing user account to user or admin.");

            routeGroup
                .MapPost("/tokens", CreateAccessTokenAsync)
                .AllowAnonymous()
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithName("CreateAccessToken")
                .WithSummary("Create an access token")
                .WithDescription("Validates user credentials and returns a short-lived bearer access token.");

            return routeGroup;
        }
    }

    private static async Task<CreatedAtRoute<CreateUserResponse>> CreateUserAsync(
        CreateUserRequest request,
        CreateUserCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateUserCommand(
            request.Username,
            request.Email,
            request.Password,
            request.Name,
            UserRole.User
        );
        var response = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.CreatedAtRoute(response, "GetUserById", new { userId = response.Id });
    }

    private static async Task<Ok<IReadOnlyList<GetUserResponse>>> GetUsersAsync(
        GetUsersQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUsersQuery();
        var response = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<GetUserResponse>> GetCurrentUserAsync(
        ClaimsPrincipal user,
        GetUserQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = user.GetUserId();
        var query = new GetUserQuery(userId);
        var response = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<GetUserResponse>> GetUserAsync(
        [Description("Unique identifier of the user to retrieve.")] Guid userId,
        GetUserQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUserQuery(userId);
        var response = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<GetUserResponse>> UpdateUserRoleAsync(
        [Description("Unique identifier of the user to update.")] Guid userId,
        UpdateUserRoleRequest request,
        ClaimsPrincipal user,
        UpdateUserRoleCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateUserRoleCommand(user.GetUserId(), userId, request.Role);
        var response = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<AuthenticateUserResponse>, UnauthorizedHttpResult>> CreateAccessTokenAsync(
        CreateAccessTokenRequest request,
        AuthenticateUserCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new AuthenticateUserCommand(request.Username, request.Password);
        var response = await handler.HandleAsync(command, cancellationToken);

        return response is null ? TypedResults.Unauthorized() : TypedResults.Ok(response);
    }
}
