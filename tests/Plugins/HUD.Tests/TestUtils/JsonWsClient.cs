using NetCoreServer;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace HUD.Tests.TestUtils
{
    internal class JsonWsClient : WsClient
    {
        public Task Connected { get { return _connectedCompletionSource.Task; } }

        private readonly TaskCompletionSource _connectedCompletionSource;
        private readonly string _path;
        private BufferBlock<string> _received;
        private TimeSpan _timeout;

        public JsonWsClient(TimeSpan timeout, int port, string path = "/") : base(IPAddress.Loopback, port)
        {
            _connectedCompletionSource = new TaskCompletionSource();
            _path = path;
            _received = new BufferBlock<string>();
            _timeout = timeout;
        }

        public bool ConnectAndWait()
        {
            return ConnectAsync() && Connected.Wait(_timeout);
        }

        public T? next<T>()
        {
            return JsonSerializer.Deserialize<T>(NextString());
        }

        public JsonDocument NextJson()
        {
            return JsonDocument.Parse(NextString());
        }

        public string NextString()
        {
            return _received.Receive(_timeout);
        }

        public override void OnWsConnecting(HttpRequest request)
        {
            request.SetBegin("GET", _path);
            request.SetHeader("Connection", "Upgrade");
            request.SetHeader("Upgrade", "websocket");
            request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
            request.SetHeader("Sec-WebSocket-Version", "13");
            request.SetBody();
        }

        public override void OnWsConnected(HttpResponse response)
        {
            _connectedCompletionSource.SetResult();
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            var message = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            _received.Post(message);
        }
    }
}
