using System.Threading.Tasks;

namespace SnowMaker
{
    public interface IOptimisticDataStore
    {
        string GetData(string blockName);
        bool TryOptimisticWrite(string blockName, string data);
    }

	public interface IOptimisticDataStoreAsync
	{
		Task<string> GetDataAsync(string blockName);
		Task<bool> TryOptimisticWriteAsync(string blockName, string data);
	}
}
