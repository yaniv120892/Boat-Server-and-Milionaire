using System;
using System.IO;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace Millionaire
{
    public class Millionare
    {
        private string _name;
        private const int Listenport = 5555;
        private UdpClient _client;
        private string _receivedData;
        private byte[] _receiveByteArray;
        private volatile bool _done;
        private NetworkStream _stream;
        private TcpClient _tcpClient;
        private Thread _recieveThread;
        private Thread _sendThread;

        private void Run()
        {
            Console.WriteLine("what is your name?");
            _name = Console.ReadLine() ?? "";
            _client = new UdpClient();
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            var ep = new IPEndPoint(IPAddress.Any, Listenport);
            _client.Client.Bind(ep);

            while (true)
            {
                try
                {
                    _done = false;
                    Console.WriteLine("[Looking for a new boat...]");

                    //getting boat broadcast
                    _receiveByteArray = _client.Receive(ref ep);
                    _receivedData = Encoding.ASCII.GetString(_receiveByteArray, 0, _receiveByteArray.Length-2);
                    
                    var shipMsg = new ShipMsg(_receivedData, ep.Address.ToString() , _receiveByteArray[_receiveByteArray.Length - 2], _receiveByteArray[_receiveByteArray.Length - 1]);

                    Console.WriteLine("[Requesting to board " + shipMsg.GetName() + "]");

                    //open connection to boat
                    _tcpClient = new TcpClient(shipMsg.GetIp(), shipMsg.GetPort());
                    _stream = _tcpClient.GetStream();

                    Console.WriteLine("[I am now aboard " + shipMsg.GetName() + "!]");

                    //getting what is your name msg
                    var bytes = new byte[256];
                    var readBytes = _stream.Read(bytes, 0, bytes.Length);
                    Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, readBytes));

                    //sending name
                    var msg = Encoding.UTF8.GetBytes(_name);
                    _stream.Write(msg, 0, msg.Length);


                    _recieveThread = new Thread(RecieveMsg);
                    _sendThread = new Thread(SendMsg);
                    _recieveThread.Start();
                    _sendThread.Start();


                    while (!_done)
                    {

                    }
                    _sendThread.Abort();
                    _recieveThread.Abort();
                    _tcpClient.Close();
                }
                catch(Exception e)
                {
                    //Console.WriteLine("got exception" + e);
                    if(_sendThread != null)
                        _sendThread.Abort();
                    if (_recieveThread != null)
                        _recieveThread.Abort();
                    if (_tcpClient != null)
                        _tcpClient.Close();
                }

            }
        }

        private void RecieveMsg()
        {
            var bytes = new byte[256];
            while (!_done)
            {
                try
                {
                    var reader = _tcpClient.GetStream();
                    var str = reader.Read(bytes, 0, bytes.Length);
                    if (str == 0)
                    {
                        _done = true;
                    }
                    var msg = Encoding.UTF8.GetString(bytes, 0, str);
                    Console.WriteLine(msg);
                }
                catch (Exception e)
                {
                    _done = true;
                }

            }
        }


        private void SendMsg()
        {
            while (true)
            {
                var amount = Console.ReadLine();
                byte[] buffer;
                if (string.IsNullOrEmpty(amount))
                {
                    buffer = Encoding.UTF8.GetBytes("\n");
                    _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
                    _done = true;
                }
                else
                {
                    buffer = Encoding.UTF8.GetBytes(amount);
                    _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }



        public static void Main(string[] args)
        {
            new Millionare().Run();
        }
    }
}