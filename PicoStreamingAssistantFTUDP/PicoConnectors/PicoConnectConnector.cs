using Microsoft.Extensions.Logging;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System.Diagnostics;
using System.Net;

namespace Pico4SAFTExtTrackingModule.PicoConnectors;

/**
 * Connector class for PICO Connect
 **/
public sealed class PicoConnectConnector : PicoConnector
{
    private const string IP_ADDRESS = "127.0.0.1";
    private const string PROCESS_NAME = "ps_server.exe";

    private ILogger Logger;

    public PicoConnectConnector(ILogger Logger)
    {
        this.Logger = Logger;
    }

    private static IPEndPoint? GetPicoConnectLocalIp()
    {
        IPEndPoint? pico_connect_target = null;

        foreach (TcpRow tcpRow in ManagedIpHelper.GetExtendedTcpTable(true))
        {
            //Logger.LogInformation(" {0,-7}{1,-23}{2, -23}{3,-14}{4}", "TCP", tcpRow.LocalEndPoint, tcpRow.RemoteEndPoint, tcpRow.State, tcpRow.ProcessId);

            try
            {
                Process process = Process.GetProcessById(tcpRow.ProcessId);
                if (process.MainModule != null && process.MainModule.FileName.EndsWith(PROCESS_NAME))
                {
                    // found
                    pico_connect_target = tcpRow.LocalEndPoint;
                    break;
                }
            }
            catch (Exception ex)
            {
                // way too much false-negatives
                //Logger.LogWarning("{exception}", ex);
            }
        }

        return pico_connect_target;
    }

    private /*static*/ PacketDevice? GetPicoConnectDevice()
    {
        IPEndPoint? picoEndpoint = GetPicoConnectLocalIp();
        if (picoEndpoint == null) return null; // couldn't find PICO Connect address

        // Retrieve the device list from the local machine
        IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
        if (allDevices.Count == 0) return null; // couldn't find any interface

        // Print the list
        for (int i = 0; i < allDevices.Count; ++i)
        {
            LivePacketDevice device = allDevices[i];
            Logger.LogInformation((i + 1) + ". " + device.Name);
            if (device.Description != null)
                Logger.LogInformation(" (" + device.Description + ")");
            else
                Logger.LogInformation(" (No description available)");
        }

        Logger.LogInformation("! checking for {}:{}", picoEndpoint.Address.ToString(), picoEndpoint.Port.ToString());

        Nullable<int> picoIndex = null;
        for (int i = 0; i < allDevices.Count && !picoIndex.HasValue; ++i)
        {
            PacketDevice current = allDevices[picoIndex.Value];
            for (int n = 0; n < current.Addresses.Count; ++n)
            {
                Logger.LogInformation("> {} -> {}", current.Addresses[n].Address.ToString(), current.Addresses[n].Destination.ToString());
            }
        }

        return allDevices[picoIndex.Value];
    }

    public bool Connect()
    {
        PacketDevice? selectedDevice = GetPicoConnectDevice();
        if (selectedDevice == null)
        {
            Logger.LogWarning("Couldn't find PICO Connect connection. Are you sure you have the app launched and you have WinPcap installed?");
            return false;
        }

        // Open the device
        using (PacketCommunicator communicator =
            selectedDevice.Open(65536,                                  // portion of the packet to capture
                                                                        // 65536 guarantees that the whole packet will be captured on all the link layers
                                PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                1000))                                  // read timeout
        {
            Logger.LogInformation("Listening on " + selectedDevice.Description + "...");

            // start the capture
            communicator.ReceivePackets(0, PacketHandler);
        }


        /*IPEndPoint? pico_connect_target = GetPicoConnectLocalIp();

        // succeed?
        if (pico_connect_target == null)
        {
            Logger.LogWarning("Couldn't find PICO Connect process.");
            return false;
        }
        Logger.LogInformation("Found PICO Connect service at IP {}.", pico_connect_target);

        endPoint = new IPEndPoint(IPAddress.Parse(IP_ADDRESS), pico_connect_target.Port);*/
        

        return true;
    }

    // Callback function invoked by Pcap.Net for every incoming packet
    private static void PacketHandler(Packet packet)
    {
        Console.WriteLine(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
    }

    public unsafe float* GetBlendShapes()
    {
        return null;
    }

    public string GetProcessName()
    {
        return "PICO Connect";
    }

    void PicoConnector.Teardown() { }
}
