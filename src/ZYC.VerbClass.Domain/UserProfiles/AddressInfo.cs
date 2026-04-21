using Volo.Abp;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.UserProfiles;

public class AddressInfo
{
    public string? Country { get; private set; }

    public string? Prefecture { get; private set; }

    public string? City { get; private set; }

    public string? Street { get; private set; }

    public string? PostalCode { get; private set; }

    public AddressInfo()
    {
    }

    public AddressInfo(
        string? country,
        string? prefecture,
        string? city,
        string? street,
        string? postalCode)
    {
        Change(country, prefecture, city, street, postalCode);
    }

    public void Change(
        string? country,
        string? prefecture,
        string? city,
        string? street,
        string? postalCode)
    {
        Country = Normalize(country, nameof(country), UserProfileConsts.MaxCountryLength);
        Prefecture = Normalize(prefecture, nameof(prefecture), UserProfileConsts.MaxPrefectureLength);
        City = Normalize(city, nameof(city), UserProfileConsts.MaxCityLength);
        Street = Normalize(street, nameof(street), UserProfileConsts.MaxStreetLength);
        PostalCode = Normalize(postalCode, nameof(postalCode), UserProfileConsts.MaxPostalCodeLength);
    }

    private static string? Normalize(string? value, string parameterName, int maxLength)
    {
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        return Check.Length(value.Trim(), parameterName, maxLength);
    }
}
