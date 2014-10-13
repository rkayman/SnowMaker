using System;

namespace SnowMaker
{
	[Serializable]
    public class UniqueIdGenerationException : Exception
    {
        public UniqueIdGenerationException(string message)
            : base(message)
        {
        }
    }
}