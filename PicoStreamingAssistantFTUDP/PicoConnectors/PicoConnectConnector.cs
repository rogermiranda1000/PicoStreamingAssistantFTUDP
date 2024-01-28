using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Pico4SAFTExtTrackingModule.PicoConnectors;

/**
 * Connector class for PICO Connect
 **/
public sealed class PicoConnectConnector : PicoConnector
{
    private const string IP_ADDRESS = "127.0.0.1";
    private const string PROCESS_NAME = "ps_server.exe";

    private ILogger Logger;

    private TcpClient? client;
    private IPEndPoint? endPoint;

    public PicoConnectConnector(ILogger Logger)
    {
        this.Logger = Logger;
    }

    public bool Connect()
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
            } catch (Exception ex)
            {
                // way too much false-negatives
                //Logger.LogWarning("{exception}", ex);
            }
        }

        // succeed?
        if (pico_connect_target == null)
        {
            Logger.LogWarning("Couldn't find PICO Connect process.");
            return false;
        }
        Logger.LogInformation("Found PICO Connect service at IP {}.", pico_connect_target);

        /*int retry = 0;

    ReInitialize:*/
        try
        {
            endPoint = new IPEndPoint(IPAddress.Parse(IP_ADDRESS), pico_connect_target.Port);
            client = new TcpClient(endPoint);

            Byte[] bytes;
            while (true)
            {
                NetworkStream stream = client.GetStream();
                if (client.ReceiveBufferSize > 0)
                {
                    bytes = new byte[client.ReceiveBufferSize];
                    stream.Read(bytes, 0, client.ReceiveBufferSize);
                    string msg = Encoding.ASCII.GetString(bytes); //the message incoming
                    Logger.LogInformation(msg);
                }
            }
        }
        /*catch (SocketException ex) when (ex.ErrorCode is 10048)
        {
            if (retry >= 3) return false;
            retry++;
            // Magic
            // Close the pico_et_ft_bt_bridge.exe process and reinitialize it.
            // It will listen to UDP port 29763 before pico_et_ft_bt_bridge.exe runs.
            // Note: exclusively to simplify older versions of the FT bridge,
            // the bridge now works without any need for process killing.
            Process proc = new()
            {
                StartInfo = {
                    FileName = "taskkill.exe",
                    ArgumentList = {
                        "/f",
                        "/t",
                        "/im",
                        PROCESS_NAME
                    },
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();
            goto ReInitialize;
        }*/
        catch (Exception e)
        {
            Logger.LogWarning("{exception}", e);
            return false;
        }

        return true;
    }

    public unsafe float* GetBlendShapes()
    {
        return null;
    }

    public string GetProcessName()
    {
        return "PICO Connect";
    }

    void PicoConnector.Teardown()
    {
        if (client is not null) client.Client.Blocking = false;
        Logger.LogInformation("Disposing of PICO Connect TCP Client.");
        client?.Dispose();
    }
}
