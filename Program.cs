using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FreeroamRaceChecker {
    class Program {
        public static bool PingHost(string nameOrAddress) {
            bool pingable = false;
            Ping pinger = null;

            try  {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            } catch (PingException) {
                // Discard PingExceptions and return false;
            } finally {
                if (pinger != null)  {
                    pinger.Dispose();
                }
            }

            return pingable;
        }

        public static bool TcpPingHost(string hostUri, int portNumber) {
            try {
                using (var client = new TcpClient(hostUri, portNumber))
                    return true;
            } catch (SocketException ex) {
                return false;
            }
        }

        public static bool UdpPingHost(string hostUri, int portNumber, byte[] packets) {
            bool returnVal;
            UdpClient udpClient;
            try {
                udpClient = new UdpClient();
                udpClient.Connect(hostUri, portNumber);
                udpClient.Client.ReceiveTimeout = 3000;
                udpClient.Send(packets, packets.Length);
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                var result = udpClient.Receive(ref RemoteIpEndPoint);
                if (result != null) {
                    Console.WriteLine(Encoding.ASCII.GetString(result));
                    returnVal = false;
                } else {
                    returnVal = true;
                }
                udpClient.Close();
                return returnVal;
            } catch (SocketException e) {
                switch (e.ErrorCode) {
                    case 10054:
                        returnVal = false;
                        break;

                    case 11001:
                        returnVal = false;
                        break;
                    default:
                        returnVal = true;
                        break;
                }
            }
            return returnVal;
        }

        static void Main(string[] args) {
            String SERVER_IP        = "145.239.5.103";
            String FREEROAM_IP      = "37.233.101.118";
            String RACE_IP          = "37.233.101.118";
            String XMPP_IP          = "145.239.5.103";

            int SERVER_PORT         = 8680;
            int FREEROAM_PORT       = 9999;
            int RACE_PORT           = 9998;
            int XMPP_PORT           = 5333;

            byte [] FREEROAM_PACKET = new byte[] {
                0x00, 0x00, 0x00, 0x06, 0x01, 0x0a, 0x0b, 0x0c, 0x0d, 0x00, 0x02, 0xda, 0xdd, 0x02, 0x00, 0x00,
                0x00, 0x86, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x6a, 0xde, 0xe7, 0x9c, 0x76, 0xbf, 0x42, 0xe2, 0x8b, 0xf2, 0xad, 0x9a,
                0x29, 0x5e, 0xf2, 0x69, 0x00, 0x00, 0xd1, 0x31, 0xdd, 0xe4, 0xd5
            };

            byte [] RACE_PACKET     = new byte[] { 
                0x00, 0x00, 0x06, 0x0a, 0x0b, 0x0c, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x5d, 0xbd, 0xc6, 0xb9, 0xb6, 0x5d, 0x48, 0xaf, 0x82, 0xcd, 0x4d, 0x9a, 0xf4, 
                0x6f, 0x48, 0x62, 0x00, 0x4b, 0x8b, 0xf5, 0x8b, 0xe4, 0x1c
            };

            String URL              = $"http://{SERVER_IP}:{SERVER_PORT}/soapbox-race-core/Engine.svc/GetServerInformation";

            Console.WriteLine("BASIC CHECKS: ");
            Console.WriteLine("\tWOPL HTTP SERVER IS " + (PingHost(SERVER_IP) ? "UP" : "DOWN"));
            Console.WriteLine("\tWOPL FREEROAM SERVER IS " + (PingHost(FREEROAM_IP) ? "UP" : "DOWN"));
            Console.WriteLine("\tWOPL RACE SERVER IS " + (PingHost(RACE_IP) ? "UP" : "DOWN"));
            Console.WriteLine("\tWOPL XMPP SERVER IS " + (PingHost(XMPP_IP) ? "UP" : "DOWN"));
            Console.WriteLine();
            Console.WriteLine("TCP CHECKS: ");
            Console.WriteLine("\tWOPL HTTP SERVER IS " + (TcpPingHost(SERVER_IP, SERVER_PORT) ? "UP" : "DOWN"));
            Console.WriteLine("\tWOPL XMPP SERVER IS " + (TcpPingHost(XMPP_IP, XMPP_PORT) ? "UP" : "DOWN"));
            Console.WriteLine();
            Console.WriteLine("UDP CHECKS: ");
            Console.WriteLine("\tWOPL FREEROAM SERVER IS " + (UdpPingHost(FREEROAM_IP, FREEROAM_PORT, FREEROAM_PACKET) ? "UP" : "DOWN"));
            Console.WriteLine("\tWOPL RACE SERVER IS " + (UdpPingHost(RACE_IP, RACE_PORT, RACE_PACKET) ? "UP" : "DOWN"));
            Console.WriteLine();
            Console.WriteLine("ALL CHECKS DONE");
            Console.ReadKey();
        }
    }
}
