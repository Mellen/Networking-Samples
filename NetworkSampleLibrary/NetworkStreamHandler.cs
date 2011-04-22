using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net.Sockets;
using System.IO;

namespace NetworkSampleLibrary
{
    public class NetworkStreamHandler
    {
        public event Action<byte[]> DataAvilable;
        public event Action<Exception, NetworkStream> StreamError;

        protected void ReadFromStream(object worker, DoWorkEventArgs args)
        {
            BackgroundWorker streamWorker = worker as BackgroundWorker;
            NetworkStream stream = args.Argument as NetworkStream;
            try
            {
                HandleStreamInput(stream);
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is ObjectDisposedException || ex is InvalidOperationException)
                {
                    streamWorker.CancelAsync();
                }

                if (ex is IOException || ex is InvalidOperationException)
                {
                    stream.Dispose();
                }

                if (StreamError != null)
                {
                    StreamError(ex, stream);
                }
            }
        }

        private void HandleStreamInput(NetworkStream stream)
        {
            int messageLength = BitConverter.ToInt32(GetBytes(stream, 4), 0);
            byte[] data = GetBytes(stream, messageLength);
            if (DataAvilable != null)
            {
                DataAvilable(data);
            }
        }

        private byte[] GetBytes(NetworkStream stream, int length)
        {
            int bytesRequired = length;
            int bytesRead = 0;
            byte[] bytes = new byte[length];
            do
            {
                int read = stream.Read(bytes, bytesRead, bytesRequired);
                bytesRequired -= read;
                bytesRead += read;
            }
            while (bytesRequired > 0);
            return bytes;
        }
    }
}
