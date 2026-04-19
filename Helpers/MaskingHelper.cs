namespace SecureVaultApp.Helpers;

public static class MaskingHelper
{
    public static string MaskOwnerId(string ownerId)
    {
        if (string.IsNullOrEmpty(ownerId)) return "usr-***";
        if (ownerId.Length <= 4) return $"usr-***{ownerId}";

        return $"usr-***{ownerId.Substring(ownerId.Length - 4)}";
    }

    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return "***@***.***";

        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return $"***{email.Substring(atIndex)}";

        var firstChar = email[0];
        var domainPart = email.Substring(atIndex);
        return $"{firstChar}***{domainPart}";
    }

    public static string MaskContent(string content, string classification)
    {
        if (classification.Equals("Confidential", StringComparison.OrdinalIgnoreCase))
        {
            return "***** CONFIDENTIAL *****";
        }

        return content ?? "No content available";
    }
}