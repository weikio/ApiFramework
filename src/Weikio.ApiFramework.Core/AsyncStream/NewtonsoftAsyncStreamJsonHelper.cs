using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Weikio.ApiFramework.Core.AsyncStream
{
    internal class NewtonsoftAsyncStreamJsonHelper : IAsyncStreamJsonHelper
    {
        private readonly MvcNewtonsoftJsonOptions _newtonsoftOptions;
        private JsonTextWriter _writer;
        private StreamWriter _streamWriter;
        private Newtonsoft.Json.JsonSerializer _serializer;

        public NewtonsoftAsyncStreamJsonHelper(IOptions<MvcNewtonsoftJsonOptions> newtonsoftOptions)
        {
            _newtonsoftOptions = newtonsoftOptions.Value;
        }

        public void Initialize(MemoryStream buffer)
        {
            _streamWriter = new StreamWriter(buffer);
            _writer = new JsonTextWriter(_streamWriter);
            _serializer = Newtonsoft.Json.JsonSerializer.Create(_newtonsoftOptions.SerializerSettings);
        }

        public void WriteStartArray()
        {
            _writer.WriteStartArray();
        }

        public void Serialize(object item)
        {
            _serializer.Serialize(_writer, item);
        }

        public void WriteEndArray()
        {
            _writer.WriteEndArray();
        }

        public async Task DisposeAsync()
        {
            await _writer.FlushAsync().ConfigureAwait(false);
        }
    }
}