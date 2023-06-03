namespace MLD.Common.Utils;

public static class ShortIdGenerator
{
    private static Random random = new Random();

    public static string GenerateShortKey(int CharacterCount = 12)
    {
        // bitCount = characterCount * log (targetBase) / log(2)
        var bitCount = 6 * CharacterCount;
        var byteCount = (int)Math.Ceiling(bitCount / 8f);
        byte[] buffer = new byte[byteCount];
        random.NextBytes(buffer);

        string guid = Convert.ToBase64String(buffer);
        // Replace URL unfriendly characters
        guid = guid.Replace('+', '-').Replace('/', '_');
        // Trim characters to fit the count
        return guid.Substring(0, CharacterCount);
    }
}
