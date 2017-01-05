using System.Linq;
using System.Security.Cryptography;

namespace ChessVariantsTraining.Services
{
    public class RandomProvider : IRandomProvider
    {
        public bool RandomBool()
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] result = new byte[1];
            rng.GetBytes(result);
            return result[0] % 2 == 0;
        }

        public string RandomString(int length)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            byte[] result = new byte[length];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(result);
            return string.Concat(result.Select(x => x % chars.Length).Select(x => chars[x]));
        }
    }
}
