using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Weikio.ApiFramework.Core.AsyncStream
{
    internal class SystemTextAsyncStreamJsonHelper : IAsyncStreamJsonHelper
    {
        private readonly JsonOptions _jsonOptions;
        private Utf8JsonWriter _writer;

        public SystemTextAsyncStreamJsonHelper(IOptions<JsonOptions> jsonOptions)
        {
            _jsonOptions = jsonOptions.Value;
        }

        public void Initialize(MemoryStream buffer)
        {
            _writer = new Utf8JsonWriter(buffer);
        }

        public void WriteStartArray()
        {
            _writer.WriteStartArray();
        }

        public void Serialize(object item)
        {
            JsonSerializer.Serialize(_writer, item, _jsonOptions.JsonSerializerOptions);
        }

        public void WriteEndArray()
        {
            _writer.WriteEndArray();
        }

        public async Task DisposeAsync()
        {
            await _writer.DisposeAsync().ConfigureAwait(false);
        }
    }
}