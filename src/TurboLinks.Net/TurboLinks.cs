using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TurboLinks.Net
{
    public class TurboLinks
    {
        readonly private RequestDelegate _next;

        public TurboLinks(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //Adding Turbolinks header to fix redirect issues in Turbolinks.
            if ((context.Response.StatusCode == 302) || (context.Response.StatusCode == 301))
            {
                context.Response.OnStarting(state =>
                {
                    var httpContext = (HttpContext)state;
                    if (!httpContext.Response.Headers.ContainsKey("Turbolinks-Location"))
                    {
                        httpContext.Response.Headers.Add("Turbolinks-Location", new[] { httpContext.Response.Headers["Location"].ToString() });
                    }
                    return Task.FromResult(0);
                }, context);
            }
            await _next(context);
        }
    }

    public static class BuilderExtension
    {
        public static IApplicationBuilder UseTurboLinks(this IApplicationBuilder app) => app.UseMiddleware<TurboLinks>();
    }

}