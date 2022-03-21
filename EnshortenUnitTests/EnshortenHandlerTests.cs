using FakeItEasy;
using Xunit;
using Enshorten;
using System.Threading.Tasks;

namespace EnshortenUnitTests
{
    public class EnshortenHandlerTests
    {
        private readonly IEnshortenRepository _enshortenRepository = A.Fake<IEnshortenRepository>();

        [Fact]
        public async Task Should_Return_Full_URL()
        {
            const string url = "www.chaosgroup.com";
            const string hash = "cHa0s";

            A.CallTo(() => _enshortenRepository.GetFullUrl((int)EnshortenUtils.Base62ToInt(hash))).Returns(url);

            var result = await new EnshortenHandler(_enshortenRepository).GetShortenedUrlAsync(hash);

            Assert.Equal(url, result);

            A.CallTo(() => _enshortenRepository.GetFullUrl((int)EnshortenUtils.Base62ToInt(hash))).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("www.google.com", 526628817)]
        [InlineData("www.chaosgroup.com", 147744966)]
        public async Task Should_Successfully_Shorten_Long_URL(string url, int crc32)
        {
            var dto = new ShortenedUrlDto(crc32, url);

            A.CallTo(() => _enshortenRepository.InsertShortenedUrl(dto)).Returns(crc32);

            var result = await new EnshortenHandler(_enshortenRepository).ShortenUrlAsync(url);

            Assert.NotEmpty(result);
            A.CallTo(() => 
                _enshortenRepository.InsertShortenedUrl(
                    A<ShortenedUrlDto>.That.Matches(x => x.Hash == crc32))
                ).MustHaveHappenedOnceExactly();
        }
    }
}