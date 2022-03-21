using System.Data.SqlClient;

namespace Enshorten
{
    public interface IEnshortenRepository
    {
        Task<string> GetFullUrl(int hash);
        Task<int> InsertShortenedUrl(ShortenedUrlDto dto);
    }

    public class EnshortenRepository : IEnshortenRepository
    {
        private readonly GetFullUrlQuery _getFullUrlQuery;
        private readonly InsertShortenedUrlCommand _insertShortenedUrlCommand;
        private readonly OpenAddressingProbingQuery _openAddressingProbingQuery;
        private readonly int _quadraticProbingThreshold = 8196;
        private readonly int _quadraticProbingStep = 2;

        public EnshortenRepository(
            GetFullUrlQuery getFullUrlQuery,
            InsertShortenedUrlCommand insertShortenedUrlCommand,
            OpenAddressingProbingQuery openAddressingProbingQuery)
        {
            _getFullUrlQuery = getFullUrlQuery;
            _insertShortenedUrlCommand = insertShortenedUrlCommand;
            _openAddressingProbingQuery = openAddressingProbingQuery;
        }

        public Task<string> GetFullUrl(int hash)
        {
            return _getFullUrlQuery.Execute(hash);
        }

        public async Task<int> InsertShortenedUrl(ShortenedUrlDto dto)
        {
            try
            {
                return await _insertShortenedUrlCommand.Execute(dto);
            }
            catch (SqlException)
            {
                if (await _getFullUrlQuery.Execute(dto.Hash) == dto.Url)
                {
                    return dto.Hash;
                }

                var probeNumber = 1;
                var ascPivot = dto.Hash;
                var descPivot = dto.Hash;

                bool isAscendingExhausted = false;
                bool isDescendingExhausted = false;
                while (true)
                {
                    if (!isAscendingExhausted)
                    {
                        var result = await _openAddressingProbingQuery.ExecuteAscending(ascPivot, probeNumber);
                        for (int i = 1; i <= probeNumber; i++)
                        {
                            var unoccupiedHash = ascPivot + i;
                            if (unoccupiedHash == EnshortenUtils.PermutationsForAllowableCharacters)
                            {
                                isAscendingExhausted = true;
                            }
                            if (!result.Contains(unoccupiedHash))
                            {
                                var updatedDto = new ShortenedUrlDto(unoccupiedHash, dto.Url);
                                return await _insertShortenedUrlCommand.Execute(updatedDto);
                            }
                        }
                    }
                    if (!isDescendingExhausted)
                    {
                        var result = await _openAddressingProbingQuery.ExecuteDescending(descPivot, probeNumber);
                        for (int i = 1; i <= probeNumber; i++)
                        {
                            var unoccupiedHash = descPivot - i;
                            if (unoccupiedHash == -1)
                            {
                                isDescendingExhausted = true;
                            }
                            if (!result.Contains(unoccupiedHash))
                            {
                                var updatedDto = new ShortenedUrlDto(unoccupiedHash, dto.Url);
                                return await _insertShortenedUrlCommand.Execute(updatedDto);
                            }
                        }
                    }
                    if (isAscendingExhausted && isDescendingExhausted)
                    {
                        return 0;
                    }

                    ascPivot += probeNumber;
                    descPivot -= probeNumber;
                    if (probeNumber < _quadraticProbingThreshold)
                    {
                        probeNumber *= _quadraticProbingStep;
                    }
                }

            }
        }
    }
}
