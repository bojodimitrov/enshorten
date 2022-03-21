using Force.Crc32;
using System.Text;

namespace Enshorten
{
    public interface IUrlShortenHandler
    {
        Task<string> ShortenUrlAsync(string url);
        Task<string> GetShortenedUrlAsync(string urlHash);
    }

    public class EnshortenHandler : IUrlShortenHandler
    {
        private readonly double PermutationsForAllowableCharacters = EnshortenUtils.PermutationsForAllowableCharacters;
        private readonly IEnshortenRepository _urlShortenerRepository;

        public EnshortenHandler(IEnshortenRepository urlShortenerRepository)
        {
            _urlShortenerRepository = urlShortenerRepository;
        }

        public async Task<string> GetShortenedUrlAsync(string urlHash)
        {
            return await _urlShortenerRepository.GetFullUrl((int)EnshortenUtils.Base62ToInt(urlHash));
        }

        public async Task<string> ShortenUrlAsync(string url)
        {
            var crc32Value = Crc32Algorithm.Compute(Encoding.ASCII.GetBytes(url));
            var crc32ValueWithinBounds = crc32Value >= PermutationsForAllowableCharacters ?
                NormalizeCRC32ValueWithinBounds(crc32Value) :
                (int)crc32Value;

            var dto = new ShortenedUrlDto(crc32ValueWithinBounds, url);

            var newHash = await _urlShortenerRepository.InsertShortenedUrl(dto);
            return EnshortenUtils.IntToBase62(newHash);
        }

        private int NormalizeCRC32ValueWithinBounds(double crc32Value)
        {
            return (int)Math.Floor((crc32Value - PermutationsForAllowableCharacters) /
                ((uint.MaxValue - PermutationsForAllowableCharacters) / PermutationsForAllowableCharacters));
        }
    }
}
