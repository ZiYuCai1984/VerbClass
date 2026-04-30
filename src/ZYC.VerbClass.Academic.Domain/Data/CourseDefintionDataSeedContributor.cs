using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;

namespace ZYC.VerbClass.Academic.Domain.Data;


internal class CourseDefintionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly CourseDefinitionSeedItem[] SeedItems =
    [
        new("52220", "電気回路基礎2"),
        new("14325", "ドイツ文化論"),
        new("15332", "日本古代史"),
        new("63591", "情報処理演習"),
        new("24682", "野外実習"),
        new("11011", "基礎英語コミュニケーション"),
        new("12024", "線形代数学"),
        new("12031", "微分積分学1"),
        new("21140", "プログラミング基礎"),
        new("21141", "データ構造とアルゴリズム"),
        new("22310", "データベース概論"),
        new("22450", "オペレーティングシステム"),
        new("31012", "マーケティング論"),
        new("32105", "会計学入門"),
        new("33170", "ミクロ経済学"),
        new("41220", "現代物理学"),
        new("42115", "有機化学"),
        new("43260", "機械設計製図"),
        new("54010", "教育心理学"),
        new("55030", "国際関係論"),
        new("66042", "画像処理"),
        new("73180", "情報セキュリティ"),
        new("84215", "地域社会学"),
        new("93240", "研究倫理")
    ];

    private readonly CourseDefinitionManager _courseDefinitionManager;
    private readonly ICourseDefinitionRepository _courseDefinitionRepository;

    public CourseDefintionDataSeedContributor(
        ICourseDefinitionRepository courseDefinitionRepository,
        CourseDefinitionManager courseDefinitionManager)
    {
        _courseDefinitionRepository = courseDefinitionRepository;
        _courseDefinitionManager = courseDefinitionManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId == null)
        {
            return;
        }

        foreach (var seedItem in SeedItems)
        {
            await CreateAndInsertAsync(seedItem.Code, seedItem.Name);
        }
    }

    private async Task CreateAndInsertAsync(string code, string name)
    {
        var definition = await _courseDefinitionRepository.FindByCodeAsync(code);
        if (definition != null)
        {
            return;
        }

        var entity = await _courseDefinitionManager.CreateAsync(code, name);
        await _courseDefinitionRepository.InsertAsync(entity, true);
    }

    private record CourseDefinitionSeedItem(string Code, string Name);
}
