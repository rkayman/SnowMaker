using System.Threading.Tasks;
using Raven.Abstractions.Data;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Raven.Imports.Newtonsoft.Json;

namespace SnowMaker
{
	public class SnowMakerDocument
	{
		public string Id { get; set; }

		public string Value { get; set; }
	
		[JsonIgnore]
		public Etag Etag { get; set; }
	}

	public class RavenOptimisticDataStore : IOptimisticDataStoreAsync
	{
		private const string SeedValue = "1";

		private readonly IDocumentStore Store;

		public RavenOptimisticDataStore(IDocumentStore store)
		{
			Store = store;
		}

		public async Task<string> GetDataAsync(string id)
		{
			var doc = await ReadDocumentAsync(id);
			return doc.Value;
		}

		public async Task<bool> TryOptimisticWriteAsync(string id, string data)
		{
			var doc = await ReadDocumentAsync(id);
			doc.Value = data;

			try
			{
				await UpdateDocumentAsync(doc);
			}
			catch (ConcurrencyException)
			{
				return false;
			}

			return true;
		}

		private async Task<SnowMakerDocument> CreateDocumentAsync(string id)
		{
			using (var session = Store.OpenAsyncSession())
			{
				session.Advanced.UseOptimisticConcurrency = true;

				var doc = new SnowMakerDocument { Id = id, Value = SeedValue };
				await session.StoreAsync(doc, id);
				await session.SaveChangesAsync();

				doc.Etag = session.Advanced.GetEtagFor(doc);

				return doc;
			}
		}

		private async Task<SnowMakerDocument> ReadDocumentAsync(string id)
		{
			using (var session = Store.OpenAsyncSession())
			{
				var doc = await session.LoadAsync<SnowMakerDocument>(id);

				if (null != doc)
					doc.Etag = session.Advanced.GetEtagFor(doc);

				return doc ?? await CreateDocumentAsync(id);
			}
		}

		private async Task UpdateDocumentAsync(SnowMakerDocument doc)
		{
			using (var session = Store.OpenAsyncSession())
			{
				session.Advanced.UseOptimisticConcurrency = true;

				await session.StoreAsync(doc, doc.Etag, doc.Id);
				await session.SaveChangesAsync();

				doc.Etag = session.Advanced.GetEtagFor(doc);
			}
		}
	}
}
