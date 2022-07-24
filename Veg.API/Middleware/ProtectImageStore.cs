using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Veg.API.Middleware
{
    public class ProtectImageStore
    {
        private readonly RequestDelegate _next;

        public ProtectImageStore(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/imagestore") && !context.User.Identity.IsAuthenticated)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
            }
            else
            {
                // Call the next delegate/middleware in the pipeline
                await _next(context);
            }
        }
    }
    public static class ProtectImageStoreMiddlewareExtensions
    {
        public static IApplicationBuilder UseProtectImageStore(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ProtectImageStore>();
        }
    }
}
