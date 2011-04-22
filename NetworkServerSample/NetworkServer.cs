using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NetworkSampleLibrary;

namespace NetworkServerSample
{
    public sealed class NetworkServer : NetworkStreamHandler, IDisposable
    {
        private bool _disposed;
        private readonly TcpListener _listener;
        private readonly List<NetworkStream> _streams = new List<NetworkStream>();   

        /// <summary>
        /// Constructs the server
        /// </summary>
        /// <param name="port">Port for the server to run on</param>
        public NetworkServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(AcceptAClient, _listener);
            DataAvilable += SendDataToAll;

            StreamError += (ex, stream) =>
                {
                    if (ex is IOException || ex is InvalidOperationException || ex is ObjectDisposedException)
                    {
                        _streams.Remove(stream);
                        Console.WriteLine("lost connection {0}", ex.GetType().Name);
                    }
                    else
                    {
                        throw ex;
                    }
                };
        }

        private void AcceptAClient(IAsyncResult asyncResult)
        {
            TcpListener listener = asyncResult.AsyncState as TcpListener;

            try
            {
                TcpClient client = listener.EndAcceptTcpClient(asyncResult);

                Console.WriteLine("Got a connection from {0}.", client.Client.RemoteEndPoint);

                HandleNewStream(client.GetStream());
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Server has shutdown.");
            }

            if (!_disposed)
            {
                listener.BeginAcceptTcpClient(AcceptAClient, listener);
            }
        }

        private void HandleNewStream(NetworkStream networkStream)
        {
            _streams.Add(networkStream);
            BackgroundWorker streamWorker = new BackgroundWorker();
            streamWorker.WorkerSupportsCancellation = true;
            streamWorker.DoWork += ReadFromStream;
            streamWorker.RunWorkerCompleted += (s, a) =>
                                                {
                                                    if (_streams.Contains(networkStream) && !a.Cancelled)
                                                    {
                                                        streamWorker.RunWorkerAsync(networkStream);
                                                    }
                                                };
            streamWorker.RunWorkerAsync(networkStream);
        }

        private void SendDataToAll(byte[] data)
        {
            foreach (var stream in _streams)
            {
                stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                stream.Write(data, 0, data.Length);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _listener.Stop();
                    foreach (var stream in _streams)
                    {
                        stream.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NetworkServer()
        {
            Dispose(false);
        }
    }
}
