using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.Data;

internal partial class SakuradaUniversitySeedData
{
    private static UniversityUserSeedItem[] CreateGeneratedAdministrativeSeedItems()
    {
        var surnames = CreateGeneratedSurnames();

        (string Kanji, string Kana, string Romanized)[] givenNames =
        [
            ("裕子", "ユウコ", "Yuko"),
            ("慎一", "シンイチ", "Shinichi"),
            ("麻衣", "マイ", "Mai"),
            ("和也", "カズヤ", "Kazuya"),
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
            "ベトナム"
        ];

        string[] departmentCodes =
        [
            "general-affairs",
            "academic-affairs",
            "student-support",
            "admissions",
            "international-exchange",
            "information-systems",
            "university-library",
            "career-center",
            "health-center",
            "affiliated-library"
        ];

        var bloodTypes = CreateGeneratedBloodTypes();
        const int surnameCount = 6;
        var generatedAdministrative = new UniversityUserSeedItem[surnameCount * givenNames.Length];

        for (var index = 0; index < generatedAdministrative.Length; index++)
        {
            var surname = surnames[index / givenNames.Length];
            var givenName = givenNames[index % givenNames.Length];
            var address = addresses[(index + 3) % addresses.Length];
            var hireYear = 2016 + index % 9;
            var birthYear = 1976 + index % 18;
            var month = (index + 2) % 12 + 1;
            var day = (index + 5) % 28 + 1;
            var userName =
                $"staff.{givenName.Romanized.ToLowerInvariant()}.{surname.Romanized.ToLowerInvariant()}.{index + 1:00}";

            generatedAdministrative[index] = new UniversityUserSeedItem(
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
                index % 2 == 0 ? Gender.Female : Gender.Male,
                bloodTypes[index % bloodTypes.Length],
                nationalities[index % nationalities.Length],
                "日本",
                address.Prefecture,
                address.City,
                $"{address.Street}{index % 4 + 1}-{index % 8 + 1}-{index % 10 + 1}",
                address.PostalCode,
                null,
                departmentCodes[index % departmentCodes.Length],
                new DateTime(hireYear, 4, 1),
                []
            );
        }

        return generatedAdministrative;
    }

    private static (string Kanji, string Kana, string Romanized)[] CreateGeneratedSurnames()
    {
        return
        [
            ("高橋", "タカハシ", "Takahashi"),
            ("中村", "ナカムラ", "Nakamura"),
            ("小林", "コバヤシ", "Kobayashi"),
            ("加藤", "カトウ", "Kato"),
            ("吉田", "ヨシダ", "Yoshida"),
            ("山口", "ヤマグチ", "Yamaguchi"),
            ("松本", "マツモト", "Matsumoto"),
            ("井上", "イノウエ", "Inoue"),
            ("木村", "キムラ", "Kimura"),
            ("林", "ハヤシ", "Hayashi")
        ];
    }

    private static (string Prefecture, string City, string Street, string PostalCode)[] CreateGeneratedAddresses()
    {
        return
        [
            ("東京都", "文京区", "本郷", "113-0033"),
            ("神奈川県", "横浜市港北区", "日吉", "223-0061"),
            ("千葉県", "船橋市", "本町", "273-0005"),
            ("埼玉県", "さいたま市浦和区", "高砂", "330-0063"),
            ("大阪府", "吹田市", "江坂町", "564-0063"),
            ("愛知県", "名古屋市千種区", "今池", "464-0850"),
            ("福岡県", "福岡市早良区", "西新", "814-0002"),
            ("宮城県", "仙台市青葉区", "中央", "980-0021"),
            ("北海道", "札幌市北区", "北七条西", "060-0807"),
            ("京都府", "京都市左京区", "田中門前町", "606-8225")
        ];
    }

    private static string[] CreateGeneratedBloodTypes()
    {
        return ["A", "B", "O", "AB"];
    }
}