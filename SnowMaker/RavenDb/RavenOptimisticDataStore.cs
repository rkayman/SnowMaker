using System.Collections.Generic;
using Raven.Abstractions.Exceptions;
using Raven.Client;

namespace SnowMaker.RavenDb
{
	public class RavenOptimisticDataStore : IOptimisticDataStore
	{
		private const string SeedValue = "1";

		private readonly IDocumentStore store;

		private readonly IDictionary<string, RavenStateDocument> openStates;
		private readonly object statesLock;

		public RavenOptimisticDataStore( IDocumentStore store )
		{
			this.store = store;
			openStates = new Dictionary<string, RavenStateDocument>();
			statesLock = new object();
		}

		public string GetData( string id )
		{
			lock (statesLock)
			{
				var state = GetStateDocument( id );

				openStates[id] = state;
	
				return state.Value;
			}
		}

		private RavenStateDocument GetStateDocument( string id )
		{
			RavenStateDocument state;

			if (openStates.TryGetValue( id, out state ))
				return state;

			return RetrieveStateDocument( id ) ?? CreateStateDocument( id );
		}

		private RavenStateDocument RetrieveStateDocument( string id )
		{
			using (var session = store.OpenSession())
			{
				session.Advanced.UseOptimisticConcurrency = true;
				var state = session.Load<RavenStateDocument>( id );

				if (null != state)
					state.Etag = session.Advanced.GetEtagFor( state );

				return state;
			}
		}

		private static RavenStateDocument CreateStateDocument( string id )
		{
			return new RavenStateDocument { Id = id, Value = SeedValue };
		}

		public bool TryOptimisticWrite( string id, string data )
		{
			using (var session = store.OpenSession())
			{
				try
				{
					var state = GetStateDocument( id );
					state.Value = data;

					session.Store( state, state.Etag, state.Id );
					session.SaveChanges();
				}
				catch (ConcurrencyException)
				{
					openStates.Remove( id );
					return false;
				}

				return true;
			}
		}
	}
}
