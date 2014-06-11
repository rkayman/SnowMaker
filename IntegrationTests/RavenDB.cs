using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Listeners;
using SnowMaker;
using SnowMaker.RavenDb;
using System;

namespace IntegrationTests.cs
{
	[TestFixture]
	public class RavenDb : Scenarios<RavenDb.TestScope>
	{
		protected override IOptimisticDataStore BuildStore( TestScope scope )
		{
			return new RavenOptimisticDataStore( scope.Repository );
		}

		protected override TestScope BuildTestScope()
		{
			return new TestScope();
		}

		public class TestScope : ITestScope
		{
			public class NoStaleQueriesListener : IDocumentQueryListener
			{
				public void BeforeQueryExecuted( IDocumentQueryCustomization queryCustomization )
				{
					queryCustomization.WaitForNonStaleResults();
				}
			}

			public TestScope()
			{
				var ticks = DateTime.UtcNow.Ticks;
				IdScopeName = string.Format( "snowmakertest{0}", ticks );

				var repository = new EmbeddableDocumentStore { RunInMemory = true };

				repository.Initialize();
				repository.Conventions.DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite;
				repository.RegisterListener( new NoStaleQueriesListener() );

				Repository = repository;
			}

			public string IdScopeName { get; private set; }

			public IDocumentStore Repository { get; private set; }

			public string ReadCurrentPersistedValue()
			{
				using (var session = Repository.OpenSession())
				{
					var state = session.Load<RavenStateDocument>( IdScopeName );
					return state.Value;
				}
			}

			public void Dispose()
			{
				if (Repository.WasDisposed)
					return;

				Repository.Dispose();
			}
		}
	}
}

