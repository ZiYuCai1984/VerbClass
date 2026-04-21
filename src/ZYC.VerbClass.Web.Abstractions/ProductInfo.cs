namespace ZYC.VerbClass.Web.Abstractions;

public static class ProductInfo
{
    public static string ProductName => "VerbClass";

    public static string PackageId => "VerbClass";

    public static string Author => "ZYC Studio";

    public static string Version => "0.0.1";

    public static string Copyright =>
        $"© 2015 - {DateTime.Now.Year} {Author}. All rights reserved.";
}