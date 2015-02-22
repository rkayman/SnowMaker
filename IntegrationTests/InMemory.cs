using System;
using System.Collections.Concurrent;
using NUnit.Framework;
using SnowMaker;

namespace IntegrationTests.cs
{
	[TestFixture]
	public class InMemory : Scenarios<InMemory.TestScope>
	{
		protected override TestScope BuildTestScope()
		{
			return new TestScope();
		}

		protected override IOptimisticDataStore BuildStore(TestScope scope)
		{
			return new InMemoryOptimisticDataStore(scope.OptimisticDictionary);
		}

		public sealed class TestScope : ITestScope
		{
			public TestScope()
			{
				var ticks = DateTime.UtcNow.Ticks;
				IdScopeName = string.Format("snowmakertest{0}", ticks);
				OptimisticDictionary = new ConcurrentDictionary<string, string>();
			}

			public string IdScopeName { get; private set; }

			public ConcurrentDictionary<string, string> OptimisticDictionary { get; private set; } 

			public string ReadCurrentPersistedValue()
			{
				string value;
				var success = OptimisticDictionary.TryGetValue(IdScopeName, out value);
				return success ? value : string.Empty;
			}

			public void Dispose()
			{
				OptimisticDictionary.Clear();
			}
		}
	}
}