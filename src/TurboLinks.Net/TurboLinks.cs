using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TurboLinks.Net
{
    public class TurboLinks
    {
        private RequestDelegate _next;

        public TurboLinks(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var memoryStream = new MemoryStream();
            var bodyStream = context.Response.Body;
            context.Response.Body = memoryStream;
            await _next?.Invoke(context);
            var request = context.Request;
            var response = context.Response;

            if(!string.IsNullOrWhiteSpace(request.Headers["X-XHR-Referer"]))
            {
                context.Response.Cookies.Append("request_method", request.Method, new CookieOptions { HttpOnly = false });
                if(context.Response.StatusCode == 301 || context.Response.StatusCode == 302)
                {
                    var uri = new Uri(response.Headers["Location"]);
                    if(uri.Host.Equals(request.Host.Value))
                    {
                        response.Headers["X-XHR-Redirected-To"] = response.Headers["Location"];
                    }
                }
            }
            memoryStream.WriteTo(bodyStream);
            await bodyStream.FlushAsync();
            memoryStream.Dispose();
            bodyStream.Dispose();
        }
    }

    public static class BuilderExtension
    {
        public static IApplicationBuilder UseTurboLinks(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TurboLinks>();
        }
    }
}
