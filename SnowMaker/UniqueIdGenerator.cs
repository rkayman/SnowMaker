using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SnowMaker
{
    public class UniqueIdGenerator : IUniqueIdGenerator
    {
	    readonly IOptimisticDataStore optimisticDataStore;
	    readonly IOptimisticDataStoreAsync optimisticDataStoreAsync;
		
        readonly IDictionary<string, ScopeState> states = new Dictionary<string, ScopeState>();
        readonly object statesLock = new object();

        int batchSize = 100;
        int maxWriteAttempts = 25;

        public UniqueIdGenerator(IOptimisticDataStore optimisticDataStore)
        {
            this.optimisticDataStore = optimisticDataStore;
        }

		public UniqueIdGenerator(IOptimisticDataStoreAsync optimisticDataStoreAsync)
		{
			this.optimisticDataStoreAsync = optimisticDataStoreAsync;
		}

        public int BatchSize
        {
            get { return batchSize; }
            set { batchSize = value; }
        }

        public int MaxWriteAttempts
        {
            get { return maxWriteAttempts; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", maxWriteAttempts, "MaxWriteAttempts must be a positive number.");

                maxWriteAttempts = value;
            }
        }

        public long NextId(string scopeName)
        {
            var state = GetScopeState(scopeName);

            lock (state.IdGenerationLock)
            {
                if (state.LastId == state.HighestIdAvailableInBatch)
                    UpdateFromSyncStore(scopeName, state);

                return Interlocked.Increment(ref state.LastId);
            }
        }

        ScopeState GetScopeState(string scopeName)
        {
            return states.GetValue(
                scopeName,
                statesLock,
                () => new ScopeState());
        }

	    async void UpdateFromSyncStore(string scopeName, ScopeState state)
        {
            var writesAttempted = 0;

            while (writesAttempted < maxWriteAttempts)
            {
                var data = await GetData(scopeName);

                long nextId;
                if (!long.TryParse(data, out nextId))
                    throw new UniqueIdGenerationException(string.Format(
                       "The id seed returned from storage for scope '{0}' was corrupt, and could not be parsed as a long. The data returned was: {1}",
                       scopeName,
                       data));

                state.LastId = nextId - 1;
                state.HighestIdAvailableInBatch = nextId - 1 + batchSize;
                var firstIdInNextBatch = state.HighestIdAvailableInBatch + 1;

                if (await TryOptimisticWrite(scopeName, firstIdInNextBatch.ToString(CultureInfo.InvariantCulture)))
                    return;

                writesAttempted++;
            }

            throw new UniqueIdGenerationException(string.Format(
                "Failed to update the data store after {0} attempts. This likely represents too much contention against the store. Increase the batch size to a value more appropriate to your generation load.",
                writesAttempted));
        }

	    async Task<string> GetData(string scopeName)
	    {
		    if (null != optimisticDataStore)
			    return optimisticDataStore.GetData(scopeName);

		    return await optimisticDataStoreAsync.GetDataAsync(scopeName);
	    }

	    async Task<bool> TryOptimisticWrite(string scopeName, string data)
	    {
		    if (null != optimisticDataStore)
			    return optimisticDataStore.TryOptimisticWrite(scopeName, data);

		    return await optimisticDataStoreAsync.TryOptimisticWriteAsync(scopeName, data);
	    }
    }
}
