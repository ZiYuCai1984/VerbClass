using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.Data;

internal partial class SakuradaUniversitySeedData
{
    private static UniversityUserSeedItem[] CreateGeneratedFacultySeedItems()
    {
        var surnames = CreateGeneratedSurnames();

        (string Kanji, string Kana, string Romanized)[] givenNames =
        [
            ("拓海", "タクミ", "Takumi"),
            ("彩香", "アヤカ", "Ayaka"),
            ("健司", "ケンジ", "Kenji"),
            ("真理", "マリ", "Mari"),
            ("悠介", "ユウスケ", "Yusuke")
        ];

        var addresses = CreateGeneratedAddresses();

        string[] nationalities =
        [
            "日本",
            "日本",
            "日本",
            "中国",
            "韓国",
            "アメリカ",
            "インド"
        ];

        string[] departmentCodes =
        [
            "department-information-engineering",
            "department-electrical-electronics-engineering",
            "department-mechanical-engineering",
            "department-mathematics",
            "department-physics",
            "department-economics",
            "department-business-administration",
            "graduate-school-engineering",
            "graduate-school-science"
        ];

        var bloodTypes = CreateGeneratedBloodTypes();
        const int surnameCount = 7;
        var generatedFaculty = new UniversityUserSeedItem[surnameCount * givenNames.Length];

        for (var index = 0; index < generatedFaculty.Length; index++)
        {
            var surname = surnames[index / givenNames.Length];
            var givenName = givenNames[index % givenNames.Length];
            var address = addresses[index % addresses.Length];
            var hireYear = 2012 + index % 12;
            var birthYear = 1969 + index % 20;
            var month = index % 12 + 1;
            var day = index % 28 + 1;
            var userName =
                $"faculty.{givenName.Romanized.ToLowerInvariant()}.{surname.Romanized.ToLowerInvariant()}.{index + 1:00}";

            generatedFaculty[index] = new UniversityUserSeedItem(
                userName,
                $"{userName}@sakurada-u.ac.jp",
                VerbClassConsts.AdminPasswordDefaultValue,
                surname.Kanji,
                givenName.Kanji,
                surname.Kana,
                givenName.Kana,
                surname.Romanized,
                givenName.Romanized,
                new DateTime(birthYear, month, day),
                index % 2 == 0 ? Gender.Male : Gender.Female,
                bloodTypes[index % bloodTypes.Length],
                nationalities[index % nationalities.Length],
                "日本",
                address.Prefecture,
                address.City,
                $"{address.Street}{index % 5 + 1}-{index % 7 + 1}-{index % 11 + 1}",
                address.PostalCode,
                null,
                departmentCodes[index % departmentCodes.Length],
                new DateTime(hireYear, 4, 1),
                []
            );
        }

        return generatedFaculty;
    }
}