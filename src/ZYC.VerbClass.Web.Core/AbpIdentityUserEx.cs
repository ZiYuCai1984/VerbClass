namespace ZYC.VerbClass.Web.Core;

public static class AbpIdentityUserEx
{
    extension(AbpIdentityUser user)
    {
        public string DisplayName => $"{user.Surname} {user.Name}";
    }
}