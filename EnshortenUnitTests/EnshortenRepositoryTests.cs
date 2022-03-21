using Enshorten;
using FakeItEasy;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace EnshortenUnitTests
{
    public class EnshortenRepositoryTests
    {
        private readonly GetFullUrlQuery _getFullUrlQuery = A.Fake<GetFullUrlQuery>();
        private readonly InsertShortenedUrlCommand _insertShortenedUrlCommand = A.Fake<InsertShortenedUrlCommand>();
        private readonly OpenAddressingProbingQuery _openAddressingProbingQuery = A.Fake<OpenAddressingProbingQuery>();

        [Fact]
        public async Task Should_Return_Inserted_Url_For_Hash()
        {
            const string url = "www.chaosgroup.com";
            const int hash = 123;

            A.CallTo(() => _getFullUrlQuery.Execute(hash)).Returns(url);

            var result = await Repository().GetFullUrl(hash);

            Assert.Equal(url, result);

            A.CallTo(() => _getFullUrlQuery.Execute(hash)).MustHaveHappenedOnceExactly();
        }


        [Fact]
        public async Task Should_Return_Successfully_Inserted_Hash_Url_Mapping()
        {
            const string url = "www.chaosgroup.com";
            const int hash = 123;
            var dto = new ShortenedUrlDto(hash, url);

            A.CallTo(() => _insertShortenedUrlCommand.Execute(dto)).Returns(hash);

            var result = await Repository().InsertShortenedUrl(dto);

            Assert.Equal(hash, result);

            A.CallTo(() =>
                _insertShortenedUrlCommand.Execute(
                    A<ShortenedUrlDto>.That.Matches(x => x.Hash == hash))
                ).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_Return_Existing_Url()
        {
            const string url = "www.chaosgroup.com";
            const int hash = 123;
            var dto = new ShortenedUrlDto(hash, url);

            A.CallTo(() => _insertShortenedUrlCommand.Execute(dto)).ThrowsAsync(Instantiate<SqlException>());
            A.CallTo(() => _getFullUrlQuery.Execute(hash)).Returns(url);

            var result = await Repository().InsertShortenedUrl(dto);

            Assert.Equal(hash, result);

            A.CallTo(() =>
                _insertShortenedUrlCommand.Execute(
                    A<ShortenedUrlDto>.That.Matches(x => x.Hash == hash))
                ).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_Probe_For_Availble_Free_Hash_Ascending()
        {
            const string url = "www.chaosgroup.com";
            const string existingUrl = "www.existing.com";
            const int hash = 123;
            const int nextAvailableHash = 124;
            var dto = new ShortenedUrlDto(hash, url);

            A.CallTo(() => _insertShortenedUrlCommand.Execute(A<ShortenedUrlDto>.Ignored))
                .ThrowsAsync(Instantiate<SqlException>())
                .Once()
                .Then
                .Returns(nextAvailableHash);
            A.CallTo(() => _getFullUrlQuery.Execute(hash)).Returns(existingUrl);
            A.CallTo(() => _openAddressingProbingQuery.ExecuteAscending(A<int>.Ignored, A<int>.Ignored)).Returns(new List<int> ());
            A.CallTo(() => _openAddressingProbingQuery.ExecuteDescending(A<int>.Ignored, A<int>.Ignored)).Returns(new List<int> { 122 });

            var result = await Repository().InsertShortenedUrl(dto);

            Assert.Equal(nextAvailableHash, result);

            A.CallTo(() => _openAddressingProbingQuery.ExecuteAscending(A<int>.Ignored, A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _openAddressingProbingQuery.ExecuteDescending(A<int>.Ignored, A<int>.Ignored)).MustNotHaveHappened();
            A.CallTo(() =>
                _insertShortenedUrlCommand.Execute(
                    A<ShortenedUrlDto>.That.Matches(x => x.Hash == hash))
                ).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_Probe_For_Availble_Free_Hash_Descending()
        {
            const string url = "www.chaosgroup.com";
            const string existingUrl = "www.existing.com";
            const int hash = 123;
            const int nextAvailableHash = 122;
            var dto = new ShortenedUrlDto(hash, url);

            A.CallTo(() => _insertShortenedUrlCommand.Execute(A<ShortenedUrlDto>.Ignored))
                .ThrowsAsync(Instantiate<SqlException>())
                .Once()
                .Then
                .Returns(nextAvailableHash);
            A.CallTo(() => _getFullUrlQuery.Execute(hash)).Returns(existingUrl);
            A.CallTo(() => _openAddressingProbingQuery.ExecuteAscending(A<int>.Ignored, A<int>.Ignored)).Returns(new List<int> { 124 });
            A.CallTo(() => _openAddressingProbingQuery.ExecuteDescending(A<int>.Ignored, A<int>.Ignored)).Returns(new List<int>());

            var result = await Repository().InsertShortenedUrl(dto);

            Assert.Equal(nextAvailableHash, result);

            A.CallTo(() => _openAddressingProbingQuery.ExecuteAscending(A<int>.Ignored, A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _openAddressingProbingQuery.ExecuteDescending(A<int>.Ignored, A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() =>
                _insertShortenedUrlCommand.Execute(
                    A<ShortenedUrlDto>.That.Matches(x => x.Hash == hash))
                ).MustHaveHappenedOnceExactly();
        }

        private EnshortenRepository Repository()
        {
            return new EnshortenRepository(_getFullUrlQuery, _insertShortenedUrlCommand, _openAddressingProbingQuery);
        }

        public static T Instantiate<T>() where T : class
        {
            return (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
        }
    }
}
