using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Abstractions.Toast;

namespace ZYC.VerbClass.Web.Toast;

public class ToastManager : IToastManager, ITransientDependency
{
    public const string ToastTempDataKey = "__VerbClass.ToastQueue";

    private const string HtmxRequestHeaderName = "HX-Request";
    private const string HtmxTriggerAfterSwapHeaderName = "HX-Trigger-After-Swap";
    private const string ToastEventName = "verbclass:toast";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public ToastManager(
        IHttpContextAccessor httpContextAccessor,
        ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }

    public void Queue(ToastMessage toast)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        if (httpContext.Request.Headers.ContainsKey(HtmxRequestHeaderName))
        {
            QueueHtmxToast(httpContext, toast);
            return;
        }

        QueuePageToast(httpContext, toast);
    }

    public void Info(string message, string? title = null)
    {
        Queue(ToastMessage.Info(message, title));
    }

    public void Warn(string message, string? title = null)
    {
        Queue(ToastMessage.Warn(message, title));
    }

    public void Error(string message, string? title = null)
    {
        Queue(ToastMessage.Error(message, title));
    }

    public void Error(Exception exception, string? title = null)
    {
        Queue(ToastMessage.Error(exception, title));
    }

    private void QueueHtmxToast(HttpContext httpContext, ToastMessage toast)
    {
        var triggerEventMap = ReadHtmxTriggerEventMap(
            httpContext.Response.Headers[HtmxTriggerAfterSwapHeaderName].ToString()
        );

        var queuedHtmxToasts = triggerEventMap.TryGetValue(ToastEventName, out var toastElement)
            ? ReadToastQueue(toastElement)
            : [];

        queuedHtmxToasts.Add(toast);
        triggerEventMap[ToastEventName] = JsonSerializer.SerializeToElement(queuedHtmxToasts, JsonSerializerOptions);

        httpContext.Response.Headers[HtmxTriggerAfterSwapHeaderName] = JsonSerializer.Serialize(
            triggerEventMap,
            JsonSerializerOptions
        );
    }

    private void QueuePageToast(HttpContext httpContext, ToastMessage toast)
    {
        var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
        var queuedPageToasts = ReadToastQueue(tempData.Peek(ToastTempDataKey) as string);
        queuedPageToasts.Add(toast);
        tempData[ToastTempDataKey] = JsonSerializer.Serialize(queuedPageToasts, JsonSerializerOptions);
    }

    private static Dictionary<string, JsonElement> ReadHtmxTriggerEventMap(string? serializedHeader)
    {
        if (string.IsNullOrWhiteSpace(serializedHeader))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                serializedHeader,
                JsonSerializerOptions
            ) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<ToastMessage> ReadToastQueue(JsonElement toastElement)
    {
        try
        {
            return toastElement.ValueKind switch
            {
                JsonValueKind.Array => JsonSerializer.Deserialize<List<ToastMessage>>(
                    toastElement.GetRawText(),
                    JsonSerializerOptions
                ) ?? [],
                JsonValueKind.Object => JsonSerializer.Deserialize<ToastMessage>(
                    toastElement.GetRawText(),
                    JsonSerializerOptions
                ) is { } toast
                    ? [toast]
                    : [],
                _ => []
            };
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<ToastMessage> ReadToastQueue(string? serializedToasts)
    {
        if (string.IsNullOrWhiteSpace(serializedToasts))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<ToastMessage>>(
                serializedToasts,
                JsonSerializerOptions
            ) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
