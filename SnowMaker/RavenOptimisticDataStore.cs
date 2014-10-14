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

	public class RavenOptimisticDataStore : IOptimisticDataStore
	{
		private const string SeedValue = "1";

		private readonly IDocumentStore Store;

		public RavenOptimisticDataStore(IDocumentStore store)
		{
			Store = store;
		}

		public string GetData(string id)
		{
			var doc = ReadDocument(id);
			return doc.Value;
		}

		public bool TryOptimisticWrite(string id, string data)
		{
			var doc = ReadDocument(id);
			doc.Value = data;

			try
			{
				UpdateDocument(doc);
			}
			catch (ConcurrencyException)
			{
				return false;
			}

			return true;
		}

		private SnowMakerDocument CreateDocument(string id)
		{
			using (var session = Store.OpenSession())
			{
				session.Advanced.UseOptimisticConcurrency = true;

				var doc = new SnowMakerDocument { Id = id, Value = SeedValue };
				session.Store(doc, id);
				session.SaveChanges();

				doc.Etag = session.Advanced.GetEtagFor(doc);

				return doc;
			}
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

		private SnowMakerDocument ReadDocument(string id)
		{
			using (var session = Store.OpenSession())
			{
				var doc = session.Load<SnowMakerDocument>(id);

				if (null != doc)
					doc.Etag = session.Advanced.GetEtagFor(doc);

				return doc ?? CreateDocument(id);
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

		private void UpdateDocument(SnowMakerDocument doc)
		{
			using (var session = Store.OpenSession())
			{
				session.Advanced.UseOptimisticConcurrency = true;

				session.Store(doc, doc.Etag, doc.Id);
				session.SaveChanges();

				doc.Etag = session.Advanced.GetEtagFor(doc);
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
