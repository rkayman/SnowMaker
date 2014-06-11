using Raven.Abstractions.Data;
using Raven.Imports.Newtonsoft.Json;

namespace SnowMaker.RavenDb
{
	public class RavenStateDocument
	{
		public string Id { get; set; }

		public string Value { get; set; }

		[JsonIgnore]
		public Etag Etag { get; set; }
	}
}
