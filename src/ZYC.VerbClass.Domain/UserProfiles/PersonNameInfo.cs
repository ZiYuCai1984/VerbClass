using Volo.Abp;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.UserProfiles;

public class PersonNameInfo
{
    public string? SurnameKanji { get; private set; }

    public string? NameKanji { get; private set; }

    public string? SurnameKana { get; private set; }

    public string? NameKana { get; private set; }

    public string? SurnameRomanized { get; private set; }

    public string? NameRomanized { get; private set; }

    public PersonNameInfo()
    {
    }

    public PersonNameInfo(
        string? surnameKanji,
        string? nameKanji,
        string? surnameKana,
        string? nameKana,
        string? surnameRomanized,
        string? nameRomanized)
    {
        Change(
            surnameKanji,
            nameKanji,
            surnameKana,
            nameKana,
            surnameRomanized,
            nameRomanized
        );
    }

    public void Change(
        string? surnameKanji,
        string? nameKanji,
        string? surnameKana,
        string? nameKana,
        string? surnameRomanized,
        string? nameRomanized)
    {
        SurnameKanji = Normalize(surnameKanji, nameof(surnameKanji));
        NameKanji = Normalize(nameKanji, nameof(nameKanji));
        SurnameKana = Normalize(surnameKana, nameof(surnameKana));
        NameKana = Normalize(nameKana, nameof(nameKana));
        SurnameRomanized = Normalize(surnameRomanized, nameof(surnameRomanized));
        NameRomanized = Normalize(nameRomanized, nameof(nameRomanized));
    }

    private static string? Normalize(string? value, string parameterName)
    {
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        return Check.Length(value.Trim(), parameterName, UserProfileConsts.MaxNamePartLength);
    }
}
