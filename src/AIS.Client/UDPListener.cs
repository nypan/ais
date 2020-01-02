using Bitbucket.AIS;
using Bitbucket.AIS.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace AIS.Client
{
    public class UDPListener
    {
        private const int listenPort = 10110;

        public static void StartListener(int port)
        {

            UdpClient listener = new UdpClient(port);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);
            var aisDecoder = new AisDecoder();
            try
            {
                while (true)
                {
                    //Console.WriteLine("Waiting for broadcast");
                    var nmeaList = new List<NmeaMessage>();
                    byte[] bytes = listener.Receive(ref groupEP);
                    var nmeaString = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    var nmeaMsg = aisDecoder.GetNmeaMessage(nmeaString);

                    nmeaList.Add(nmeaMsg);
                    while (nmeaMsg.NumberOfSentences > 1 && nmeaMsg.SentenceNumber != nmeaMsg.NumberOfSentences)
                    {
                        bytes = listener.Receive(ref groupEP);
                        nmeaString = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                        nmeaMsg = aisDecoder.GetNmeaMessage(nmeaString);
                        nmeaList.Add(nmeaMsg);
                    }

                    var msg = aisDecoder.GetAisMessage(nmeaList);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(msg.ToString());
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }

    }
}
