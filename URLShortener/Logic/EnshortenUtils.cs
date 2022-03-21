using System.Text;

namespace Enshorten
{
    public static class EnshortenUtils
    {
        private static readonly char[] _chars = new char[] { '0','1','2','3','4','5','6','7','8','9',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
        private static readonly uint AllowedHashLength = 5;

        public static double PermutationsForAllowableCharacters => IntPower(_chars.Length, AllowedHashLength);

        public static string IntToBase62(long value)
        {
            var stringBuilder = new StringBuilder();
            var targetBase = _chars.Length;

            do
            {
                stringBuilder.Insert(0, _chars[value % targetBase]);
                value = value / targetBase;
            }
            while (value > 0);

            return stringBuilder.ToString();
        }

        public static long Base62ToInt(string number)
        {
            long result = 0;
            long multiplier = 1;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                char c = number[i];

                int digit = Array.IndexOf(_chars, c);

                result += digit * multiplier;
                multiplier *= _chars.Length;
            }

            return result;
        }

        static int IntPower(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }
    }
}
