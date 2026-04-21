using System.Reflection;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZYC.VerbClass.Web;

public partial class VerbClassWebModule
{
    private static void ConfigurePageRoutes(RazorPagesOptions options)
    {
        var routeFields = typeof(Routes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
            .OrderBy(field => field.Name, StringComparer.Ordinal);

        foreach (var routeField in routeFields)
        {
            var route = routeField.GetRawConstantValue() as string;
            if (string.IsNullOrWhiteSpace(route))
            {
                continue;
            }

            var pagePath = routeField.GetCustomAttribute<PagePathAttribute>()?.Path;
            if (string.IsNullOrWhiteSpace(pagePath))
            {
                pagePath = route.Replace("-", string.Empty);
            }

            if (string.Equals(route, pagePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            options.Conventions.AddPageRoute(pagePath, route);
        }
    }
}