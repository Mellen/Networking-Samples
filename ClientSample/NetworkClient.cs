using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using NetworkSampleLibrary;

namespace ClientSample
{
    public sealed class NetworkClient : NetworkStreamHandler, IDisposable
    {
        private bool _disposed;
        private readonly NetworkStream _stream;
        private bool _connected;

        public NetworkClient(System.Net.IPAddress serverAddress, int port)
        {
            TcpClient client = new TcpClient();
            client.Connect(serverAddress, port);
            _stream = client.GetStream();
            _connected = true;
            BackgroundWorker streamWorker = new BackgroundWorker();
            streamWorker.WorkerSupportsCancellation = true;
            streamWorker.DoWork += ReadFromStream;
            streamWorker.RunWorkerCompleted += (s, a) =>
                                            {
                                                if (_connected)
                                                {
                                                    streamWorker.RunWorkerAsync(_stream);
                                                }
                                            };
            streamWorker.RunWorkerAsync(_stream);
            StreamError += (ex, stream) =>
                            {
                                if (ex is IOException || ex is InvalidOperationException || ex is ObjectDisposedException)
                                {
                                    _connected = false;
                                    Console.WriteLine("Lost connection: {0}", ex.Message);
                                    Console.Write("> ");
                                }
                                else
                                {
                                    throw ex;
                                }
                            };
        }

        public void SendData(byte[] data)
        {
            _stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            _stream.Write(data, 0, data.Length);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stream.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NetworkClient()
        {
            Dispose(false);
        }
    }
}
