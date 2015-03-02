//using System.Threading.Tasks;
//using Raven.Abstractions.Data;
//using Raven.Abstractions.Exceptions;
//using Raven.Client;
//using Raven.Imports.Newtonsoft.Json;

//namespace SnowMaker
//{
//    public class SnowMakerDocument
//    {
//        public string Id { get; set; }

//        public string Value { get; set; }
	
//        [JsonIgnore]
//        public Etag Etag { get; set; }
//    }

//    public class RavenOptimisticDataStore : IOptimisticDataStoreAsync
//    {
//        private readonly IDocumentStore Store;

//        public RavenOptimisticDataStore(IDocumentStore store)
//        {
//            Store = store;
//        }

//        public async Task<string> GetDataAsync(string id)
//        {
//            using (var session = Store.OpenAsyncSession())
//            {
//                var doc = await session.ReadDocumentAsync(id);
//                return doc.Value;				
//            }
//        }

//        public async Task<bool> TryOptimisticWriteAsync(string id, string data)
//        {
//            using (var session = Store.OpenAsyncSession())
//            {
//                var doc = await session.ReadDocumentAsync(id);
//                doc.Value = data;

//                try
//                {
//                    await session.UpdateDocumentAsync(doc);
//                }
//                catch (ConcurrencyException)
//                {
//                    return false;
//                }

//                return true;
//            }
//        }
//    }

//    internal static class RavenSessionExtensions
//    {
//        private const string SeedValue = "1";

//        internal static async Task<SnowMakerDocument> CreateDocumentAsync(this IAsyncDocumentSession session, string id)
//        {
//            session.Advanced.UseOptimisticConcurrency = true;

//            var doc = new SnowMakerDocument { Id = id, Value = SeedValue };
//            await session.StoreAsync(doc, id);
//            await session.SaveChangesAsync();

//            doc.Etag = session.Advanced.GetEtagFor(doc);

//            return doc;
//        }

//        internal static async Task<SnowMakerDocument> ReadDocumentAsync(this IAsyncDocumentSession session, string id)
//        {
//            var doc = await session.LoadAsync<SnowMakerDocument>(id);

//            if (null != doc)
//                doc.Etag = session.Advanced.GetEtagFor(doc);

//            return doc ?? await session.CreateDocumentAsync(id);
//        }

//        internal static async Task UpdateDocumentAsync(this IAsyncDocumentSession session, SnowMakerDocument doc)
//        {
//            session.Advanced.UseOptimisticConcurrency = true;

//            await session.StoreAsync(doc, doc.Etag, doc.Id);
//            await session.SaveChangesAsync();

//            doc.Etag = session.Advanced.GetEtagFor(doc);
//        }
//    }
//}
