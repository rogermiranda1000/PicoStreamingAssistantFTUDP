﻿using Microsoft.Extensions.Logging;
using Pico4SAFTExtTrackingModule.PacketLogger;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking;

namespace Pico4SAFTExtTrackingModule.PicoConnectors;

/**
 * Connector class for Streaming Assitant & Business Streaming
 **/
public sealed class StreamingAssistantConnector : PicoConnector
{
    private const string IP_ADDRESS = "127.0.0.1";
    private const int PORT_NUMBER = 29765;
    private const string PROCESS_NAME = "pico_et_ft_bt_bridge.exe";

    private static readonly unsafe int pxrHeaderSize = sizeof(TrackingDataHeader);
    private readonly int PacketIndex = pxrHeaderSize;
    private static readonly unsafe int pxrFtInfoSize = sizeof(PxrFTInfo);
    private static readonly int PacketSize = pxrHeaderSize + pxrFtInfoSize;

    private ILogger Logger;
    private UdpClient? udpClient;
    private IPEndPoint? endPoint;
    private PxrFTInfo data;

    private string processName;

    public StreamingAssistantConnector(ILogger Logger, bool using_sa)
    {
        this.Logger = Logger;
        this.processName = (using_sa ? "Streaming Assistant" : "Business Streaming");
    }

    public bool Connect()
    {
        int retry = 0;

    ReInitialize:
        try
        {
            udpClient = new UdpClient(PORT_NUMBER);
            endPoint = new IPEndPoint(IPAddress.Parse(IP_ADDRESS), PORT_NUMBER);
            // Since Streaming Assistant is already running,
            // this module is indeed needed,
            // so the timeout failure is unnecessary.
            // udpClient.Client.ReceiveTimeout = 15000; // Initialization timeout.

            Logger.LogDebug("Host end-point: {endPoint}", endPoint);
            Logger.LogDebug("Initialization Timeout: {timeout}ms", udpClient.Client.ReceiveTimeout);
            Logger.LogDebug("Client established: attempting to receive PxrFTInfo.");

            Logger.LogInformation("Waiting for {} data stream.", this.processName);
            unsafe
            {
                fixed (PxrFTInfo* pData = &data)
                    ReceivePxrData(pData);
            }
            Logger.LogDebug("{} handshake success.", this.processName);
        }
        catch (SocketException ex) when (ex.ErrorCode is 10048)
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
        }
        catch (Exception e)
        {
            Logger.LogWarning("{exception}", e);
            return false;
        }

        udpClient.Client.ReceiveTimeout = 5000;

        return true;
    }

    public unsafe float* GetBlendShapes()
    {
        fixed (PxrFTInfo* pData = &data)
            if (ReceivePxrData(pData))
            {
                float* pxrShape = pData->blendShapeWeight;
                return pxrShape;
            }

        return null;
    }

    public void Teardown()
    {
        if (udpClient is not null) udpClient.Client.Blocking = false;
        Logger.LogInformation("Disposing of PxrFaceTracking UDP Client.");
        udpClient?.Dispose();
    }

    private unsafe bool ReceivePxrData(PxrFTInfo* pData)
    {
        fixed (byte* ptr = udpClient!.Receive(ref endPoint))
        {
            TrackingDataHeader tdh;
            Buffer.MemoryCopy(ptr, &tdh, pxrHeaderSize, pxrHeaderSize);
            if (tdh.tracking_type != 2) return false; // not facetracking packet

            Buffer.MemoryCopy(ptr + PacketIndex, pData, pxrFtInfoSize, pxrFtInfoSize);
        }
        return true;
    }

    public string GetProcessName()
    {
        return this.processName;
    }
}
