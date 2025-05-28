using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;

namespace Library.Api
{
    public static class ResultExtensions
    {
        public static IResult Html(this IResultExtensions extensions, string html)
        {
            return new HtmlResult(html);
        }
    }
    public class HtmlResult : IResult
    {
        readonly string _html;
        public HtmlResult(string html)
        {
            _html = html;
        }
        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = MediaTypeNames.Text.Html;
            httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
            return httpContext.Response.WriteAsync(_html, Encoding.UTF8);
        }
    }
}
