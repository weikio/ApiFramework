using System.IO;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Core.AsyncStream
{
    internal interface IAsyncStreamJsonHelper
    {
        void WriteStartArray();
        void Serialize(object item);
        void WriteEndArray();
        Task DisposeAsync();
        void Initialize(MemoryStream buffer);
    }
}
