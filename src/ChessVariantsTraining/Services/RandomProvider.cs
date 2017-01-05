using System;
using System.Linq;
using System.Security.Cryptography;

namespace ChessVariantsTraining.Services
{
    public class RandomProvider : IRandomProvider
    {
        RandomNumberGenerator rng;

        public RandomProvider()
        {
            rng = RandomNumberGenerator.Create();
        }
        public bool RandomBool()
        {
            byte[] result = new byte[1];
            rng.GetBytes(result);
            return result[0] % 2 == 0;
        }

        public string RandomString(int length)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            byte[] result = new byte[length];
            rng.GetBytes(result);
            return string.Concat(result.Select(x => x % chars.Length).Select(x => chars[x]));
        }

        public int RandomPositiveInt(int maxExclusive)
        {
            byte[] result = new byte[4];
            rng.GetBytes(result);
            return ((int)Math.Abs(BitConverter.ToInt32(result, 0))) % maxExclusive;
        }
    }
}
