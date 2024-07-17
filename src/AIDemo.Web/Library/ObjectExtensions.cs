

using System.Text.Json;

namespace Haack.AIDemoWeb.Library;

public static class ObjectExtensions
{
    public static string ToJson(this object o)
    {
        return JsonSerializer.Serialize(o);
    }
}