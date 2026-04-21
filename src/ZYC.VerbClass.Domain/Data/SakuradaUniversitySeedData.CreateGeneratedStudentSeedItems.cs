using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.Data;

internal partial class SakuradaUniversitySeedData
{
    private static UniversityUserSeedItem[] CreateGeneratedStudentSeedItems()
    {
        var surnames = CreateGeneratedSurnames();

        (string Kanji, string Kana, string Romanized)[] givenNames =
        [
            ("翔太", "ショウタ", "Shota"),
            ("大和", "ヤマト", "Yamato"),
            ("巧", "タクミ", "Takumi"),
            ("美咲", "ミサキ", "Misaki"),
            ("彩花", "アヤカ", "Ayaka"),
            ("奈々", "ナナ", "Nana")
        ];

        var addresses = CreateGeneratedAddresses();

        string[] nationalities =
        [
            "日本",
            "日本",
            "日本",
            "中国",
            "韓国",
            "台湾",
            "ベトナム"
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

        string[] bloodTypes = ["A", "B", "O", "AB"];
        var generatedStudents = new UniversityUserSeedItem[surnames.Length * givenNames.Length];

        for (var index = 0; index < generatedStudents.Length; index++)
        {
            var surname = surnames[index / givenNames.Length];
            var givenName = givenNames[index % givenNames.Length];
            var address = addresses[index % addresses.Length];
            var enrollmentYear = 2022 + index % 4;
            var birthYear = enrollmentYear - 19;
            var month = index % 12 + 1;
            var day = index % 28 + 1;
            var userName =
                $"{givenName.Romanized.ToLowerInvariant()}.{surname.Romanized.ToLowerInvariant()}.{index + 1:00}";

            generatedStudents[index] = new UniversityUserSeedItem(
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
                $"{address.Street}{index % 6 + 1}-{index % 9 + 1}-{index % 15 + 1}",
                address.PostalCode,
                enrollmentYear,
                departmentCodes[index % departmentCodes.Length],
                new DateTime(enrollmentYear, 4, 1),
                []
            );
        }

        return generatedStudents;
    }
}