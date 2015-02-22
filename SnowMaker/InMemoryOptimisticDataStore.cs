using System.Collections.Concurrent;

namespace SnowMaker
{
	public class InMemoryOptimisticDataStore : IOptimisticDataStore
	{
		private const string SeedValue = "1";

		private readonly ConcurrentDictionary<string, string> OptimisticDictionary;

		public InMemoryOptimisticDataStore()
		{
			OptimisticDictionary = new ConcurrentDictionary<string, string>();
		}

		public InMemoryOptimisticDataStore(ConcurrentDictionary<string, string> optimisticDictionary)
		{
			OptimisticDictionary = optimisticDictionary;
		}

		public string GetData(string blockName)
		{
			return OptimisticDictionary.GetOrAdd(blockName, SeedValue);
		}

		public bool TryOptimisticWrite(string blockName, string data)
		{
			return OptimisticDictionary.AddOrUpdate(blockName, data, (key, value) => data).Equals(data);
		}
	}
}