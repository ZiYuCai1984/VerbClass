namespace ZYC.VerbClass.Web.Abstractions;

[AttributeUsage(AttributeTargets.Field)]
public class PagePathAttribute : Attribute
{
    public string Path { get; }

    public PagePathAttribute(string path) => Path = path;

}