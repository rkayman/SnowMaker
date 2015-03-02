//using System;
//using NUnit.Framework;
//using Raven.Client;
//using Raven.Client.Document;
//using Raven.Client.Embedded;
//using Raven.Client.Listeners;
//using SnowMaker;

//namespace IntegrationTests.cs
//{
//    [TestFixture]
//    public class Raven : ScenariosAsync<Raven.TestScope>
//    {
//        protected override IOptimisticDataStoreAsync BuildStore(TestScope scope)
//        {
//            return new RavenOptimisticDataStore(scope.Store);
//        }

//        protected override TestScope BuildTestScope()
//        {
//            return new TestScope();
//        }

//        public sealed class TestScope : ITestScope
//        {
//            public sealed class NoStaleQueriesListener : IDocumentQueryListener
//            {
//                public void BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization)
//                {
//                    queryCustomization.WaitForNonStaleResults();
//                }
//            }

//            public TestScope()
//            {
//                var ticks = DateTime.UtcNow.Ticks;
//                IdScopeName = String.Format("snowmakertest{0}", ticks);

//                var store = new EmbeddableDocumentStore { RunInMemory = true };
//                store.Initialize();
//                store.Conventions.DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite;
//                store.RegisterListener(new NoStaleQueriesListener());

//                Store = store;
//            }

//            public void Dispose()
//            {
//                if (!Store.WasDisposed)
//                    Store.Dispose();
//            }

//            public string IdScopeName { get; private set; }

//            public IDocumentStore Store { get; private set; }

//            public string ReadCurrentPersistedValue()
//            {
//                using (var session = Store.OpenSession())
//                {
//                    var doc = session.Load<SnowMakerDocument>(IdScopeName);
//                    return doc.Value;
//                }
//            }
//        }
//    }
//}