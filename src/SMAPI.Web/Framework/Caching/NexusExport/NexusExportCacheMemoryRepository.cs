using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using StardewModdingAPI.Toolkit.Framework.Clients.NexusExport;
using StardewModdingAPI.Toolkit.Framework.Clients.NexusExport.ResponseModels;

namespace StardewModdingAPI.Web.Framework.Caching.NexusExport
{
    /// <summary>Manages cached mod data from the Nexus export API in-memory.</summary>
    internal class NexusExportCacheMemoryRepository : BaseCacheRepository, INexusExportCacheRepository
    {
        /*********
        ** Fields
        *********/
        /// <summary>The cached mod data from the Nexus export API.</summary>
        private NexusFullExport? Data;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        [MemberNotNullWhen(true, nameof(NexusExportCacheMemoryRepository.Data))]
        public bool IsLoaded()
        {
            return this.Data?.Data.Count > 0;
        }

        /// <inheritdoc />
        public async Task<bool> CanRefreshFromAsync(INexusExportApiClient client, int staleMinutes)
        {
            DateTimeOffset serverLastModified = await client.FetchLastModifiedDateAsync();

            return
                !this.IsStale(serverLastModified, staleMinutes)
                && (
                    !this.IsLoaded()
                    || this.Data.LastUpdated < serverLastModified
                );
        }

        /// <inheritdoc />
        public bool TryGetMod(uint id, [NotNullWhen(true)] out NexusModExport? mod)
        {
            var data = this.Data?.Data;

            if (data is null || !data.TryGetValue(id, out mod))
            {
                mod = null;
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void SetData(NexusFullExport? export)
        {
            this.Data = export;
        }

        /// <inheritdoc />
        public bool IsStale(int staleMinutes)
        {
            DateTimeOffset? lastUpdated = this.Data?.LastUpdated;
            return lastUpdated.HasValue && this.IsStale(lastUpdated.Value, staleMinutes);
        }
    }
}
