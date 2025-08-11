using Microsoft.AspNetCore.Mvc;

public static class RedirectHelper
{
    /// <summary>
    /// Returns a local redirect if the url is local, otherwise returns a default safe path.
    /// </summary>
    public static IActionResult SafeLocalRedirect(string? returnUrl, string defaultPath = "/")
    {
        if (!string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) && !returnUrl.StartsWith("//"))
        {
            return new RedirectResult(returnUrl);
        }
        // Optionally log or handle suspicious redirect attempts here
        return new RedirectResult(defaultPath);
    }
}