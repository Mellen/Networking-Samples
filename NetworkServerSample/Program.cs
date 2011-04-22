using System;
using System.Text;

namespace NetworkServerSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NetworkServer server = new NetworkServer(12345))
            {
                server.DataAvilable += data => Console.WriteLine(Encoding.UTF8.GetString(data));
                Console.WriteLine("Press enter to exit (how screwy is that?)");
                Console.ReadLine();
            }
            
        }
    }
}
