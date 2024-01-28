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
    private ILogger Logger;

    public PicoConnectConnector(ILogger Logger)
    {
        this.Logger = Logger;
    }

    public bool Connect()
    {
        //Nullable<int> pico_connect_pid = null;
        IPEndPoint? pico_connect_target = null;

        foreach (TcpRow tcpRow in ManagedIpHelper.GetExtendedTcpTable(true))
        {
            //Logger.LogInformation(" {0,-7}{1,-23}{2, -23}{3,-14}{4}", "TCP", tcpRow.LocalEndPoint, tcpRow.RemoteEndPoint, tcpRow.State, tcpRow.ProcessId);

            try
            {
                Process process = Process.GetProcessById(tcpRow.ProcessId);
                if (process.MainModule != null && process.MainModule.FileName.EndsWith("Streaming Service\\ps_server.exe"))
                {
                    // found
                    //pico_connect_pid = tcpRow.ProcessId;
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
        if (/*!pico_connect_pid.HasValue ||*/ pico_connect_target == null) return false;
        Logger.LogInformation("Found PICO Connect service at IP {}.", pico_connect_target);

        TcpListener listen = new TcpListener(pico_connect_target);
        listen.Start();
        Byte[] bytes;
        try
        {
            while (true)
            {
                TcpClient client = listen.AcceptTcpClient();
                NetworkStream ns = client.GetStream();
                if (client.ReceiveBufferSize > 0)
                {
                    bytes = new byte[client.ReceiveBufferSize];
                    ns.Read(bytes, 0, client.ReceiveBufferSize);
                    string msg = Encoding.ASCII.GetString(bytes); //the message incoming
                    Logger.LogInformation(msg);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning("{exception}", ex);
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
        
    }
}
