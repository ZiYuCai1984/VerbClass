using ZYC.VerbClass.Web.Abstractions.MainMenu;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.MainMenu;

public partial class MainMenuManager
{
    private class VisibleMainMenuItem : MainMenuItem
    {
        public VisibleMainMenuItem(IMainMenuItem source, IMainMenuItem[] subItems)
        {
            Source = source;
            Href = source.Href;
            SubItems = subItems;
            Title = source.Title;
            Icon = source.Icon;
            Anchor = source.Anchor;
            Priority = source.Priority;
            Localization = source.Localization;
        }

        private IMainMenuItem Source { get; }

        public override string Href { get; }

        public override IMainMenuItem[] SubItems { get; }

        public override string Title { get; }

        public override string? Icon { get; }

        public override string Anchor { get; }

        public override int Priority { get; }

        public override bool Localization { get; }

        public override Task<bool> IsVisibleAsync(UserTenantContext context)
        {
            return Source.IsVisibleAsync(context);
        }
    }
}