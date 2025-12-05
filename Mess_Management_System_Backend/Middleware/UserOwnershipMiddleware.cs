using System.Security.Claims;

namespace Mess_Management_System_Backend.Middleware
{
    public class UserOwnershipMiddleware
    {
        private readonly RequestDelegate _next;

        public UserOwnershipMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only check for routes that have {id} parameter and require authentication
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var path = context.Request.Path.Value?.ToLower();
                
                // Check if this is a user-specific endpoint (contains /users/{id})
                if (path != null && path.Contains("/api/users/") && context.Request.RouteValues.ContainsKey("id"))
                {
                    var routeId = context.Request.RouteValues["id"]?.ToString();
                    
                    if (int.TryParse(routeId, out int requestedUserId))
                    {
                        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var isAdmin = context.User.IsInRole("Admin");

                        if (userIdClaim != null && int.TryParse(userIdClaim, out int loggedInUserId))
                        {
                            var method = context.Request.Method;
                            
                            // Define which requests need ownership validation
                            var isModifyingRequest = method == "PUT" || method == "PATCH" || method == "DELETE";
                            var isPasswordChange = path.Contains("/change-password") && method == "POST";
                            
                            // Enforce ownership for:
                            // 1. PUT/PATCH/DELETE requests
                            // 2. Password change requests
                            // Unless user is Admin
                            if ((isModifyingRequest || isPasswordChange) && !isAdmin && loggedInUserId != requestedUserId)
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsJsonAsync(new 
                                { 
                                    message = "You are not authorized to modify this resource" 
                                });
                                return;
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method to easily add middleware to pipeline
    public static class UserOwnershipMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserOwnershipValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserOwnershipMiddleware>();
        }
    }
}
