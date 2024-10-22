
public class IDGenerator
{
    private static System.Random random = new System.Random();

    public static string GenerateStringID(int countSymbol)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] idChars = new char[countSymbol];

        for (int i = 0; i < countSymbol; i++)
        {
            idChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(idChars);
    }

    public static int GenerateNumberID(int countSymbol)
    {
        int randomNumb = 0;

        for (int i = 0; i < countSymbol; i++)
        {
            randomNumb += random.Next(0, countSymbol);
        }

        return randomNumb;


    }
}
