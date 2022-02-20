namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtension
    {
        public static string? EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Page("/Identity/Account/ConfirmEmail", null, new { userId, code }, scheme);
            //return urlHelper.Action(
            //    action: nameof(AccountController.ConfirmEmail),
            //    controller: "Account",
            //    values: new { userId, code },
            //    protocol: scheme);
        }

        public static string? ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Page("/Identity/Account/ResetPassword", null, new { userId, code }, scheme);
            //return urlHelper.Action(
            //    action: nameof(AccountController.ResetPassword),
            //    controller: "Account",
            //    values: new { userId, code },
            //    protocol: scheme);
        }
    }
}
