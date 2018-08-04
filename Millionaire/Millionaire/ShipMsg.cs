using System;
using System.Linq;
using System.Text;

namespace Millionaire
{
    public class ShipMsg
    {
        private string name;
        private int port;
        private string ip;

        public string GetIp()
        {
            return ip;
        }
        public string GetName()
        {
            return name;
        }

        public int GetPort()
        {
            return port;
        }

        public ShipMsg(string msg , string ip, byte first , byte second)
        {
            if(msg.Length != 43)
                throw new Exception();
            if(msg.Substring(0,11) != "IntroToNets")
                throw new Exception();
            name = msg.Substring(11, 32).Replace(" ", "");

            string firstDec = first.ToString();
            string secondDec = second.ToString();


            string firstHex = (int.Parse(firstDec)).ToString("X");
            string secondHex = (int.Parse(secondDec)).ToString("X");

            string hex = firstHex + secondHex;

            port = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);


            this.ip = ip;
        }
    }
}