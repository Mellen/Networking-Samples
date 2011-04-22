using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ClientSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NetworkClient client = new NetworkClient(IPAddress.Parse("127.0.0.1"), 12345))
            {
                client.DataAvilable += data =>
                    {
                        string message = Encoding.UTF8.GetString(data);
                        Console.WriteLine("Message from server: {0}", message);
                        Console.Write("> ");
                    };

                Console.Write("> ");
                string input = Console.ReadLine();

                while (input != "q")
                {
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        client.SendData(Encoding.UTF8.GetBytes(input));
                    }
                    Console.Write("> ");
                    input = Console.ReadLine();
                }
            }
        }
    }
}
