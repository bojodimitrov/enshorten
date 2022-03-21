using Enshorten;
using Xunit;

namespace EnshortenUnitTests
{
    public class EnshortenUtilsTests
    {
        [Theory]
        [InlineData(1, "1")]
        [InlineData(61, "z")]
        [InlineData(0, "0")]
        [InlineData(10069001, "gFPF")]
        [InlineData(999999, "4C91")]
        [InlineData(57896541321, "11CBzCb")]
        public void Should_Convert_Integer_To_Base62(long integer, string expectedBase62)
        {
            var result = EnshortenUtils.IntToBase62(integer);

            Assert.Equal(expectedBase62, result);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("z", 61)]
        [InlineData("0", 0)]
        [InlineData("gFPF", 10069001)]
        [InlineData("4C91", 999999)]
        [InlineData("11CBzCb", 57896541321)]
        public void Should_Convert_Base62_To_Integer(string base62, long expectedInteger)
        {
            var result = EnshortenUtils.Base62ToInt(base62);

            Assert.Equal(expectedInteger, result);
        }
    }
}
