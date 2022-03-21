namespace Enshorten
{
    public class ShortenedUrlDto
    {
        public int Hash { get; }
        public string Url { get; }

        public ShortenedUrlDto(int hash, string url)
        {
            Hash = hash;
            Url = url;
        }
    }
}
