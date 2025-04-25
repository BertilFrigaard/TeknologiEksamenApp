namespace TeknologiEksamenApp.Utils;

public static class InputVerifier
{
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        // TODO: Implement regex for email validation
        return true;
    }

    public static bool IsValidPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return false;
        }

        // TODO: Implement regex and length check for password validation
        return true;
    }

    public static bool IsValidName(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        // TODO: Implement regex to check for invalid characters in name
        return true;
    }

    public static bool IsValidAmount(string? amount)
    {
        if (string.IsNullOrEmpty(amount))
        {
            return false;
        }
        try
        {
            float.Parse(amount);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidJoinCode(string? joinCode)
    {
        return string.IsNullOrEmpty(joinCode) == false && joinCode.Length == 6;
    }
}
