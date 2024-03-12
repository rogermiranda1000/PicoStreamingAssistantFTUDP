## English || [简体中文](README_CN.md)

# VRCFaceTracking - Pico 4 Pro Module

> Let VRCFaceTracking use the Pico 4 Pro's face tracking data via the Pico Streaming Assistant.

## Setup **Pico 4 Pro Module** for **VRCFaceTracking**.

1. Download the latest Streaming Assistant, Business Streaming, or PICO Connect
2. Download and install the Pico 4 Pro Module.
* [Latest Release](https://github.com/regzo2/PicoStreamingAssistantFTUDP/releases)
  * Installer
    * (TBD) Simply install **Pico 4 Pro Module** from VRCFaceTracking's **Module Registry** tab.
  * Manual
    * Include the supplied **.dll** release in `%appdata%/VRCFaceTracking/CustomLibs`. 
3. (If you're using PICO Connect) Install [WinPcap 4.1.3](https://www.winpcap.org/install/bin/WinPcap_4_1_3.exe)
**VRCFaceTracking v5.0.0.0 is required to use Pico 4 Pro Module.**

3. Launch VRCFaceTracking!

## Troubleshooting

> The Pico 4 Pro is not connecting to VRCFaceTracking using the module.

Make sure that your Pico 4 device is capable of using face tracking. 
At this time only the Pico 4 Pro is capable of being used with this module.
Make sure you have the latest streaming app capable of sending face tracking data.
(If you're using PICO Connect) Make sure you have WinPcap 4.1.3

  
## Licenses / Distribution

**(TBD)**.

## Compiling this module
- Use [Visual Studio 2022](https://visualstudio.microsoft.com/es/vs/)
- Clone [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking) in the same folder as this project
- Download [Pcap.Net v1.0.4 Binaries](https://github.com/PcapDotNet/Pcap.Net/releases/tag/v1.0.4) and unzip `PcapDotNet.Binaries.1.0.4` at the base project path
- Compile VRCFaceTracking.Core

You'll find the module in `PicoStreamingAssistantFTUDP\PicoStreamingAssistantFTUDP\bin\Debug\net7.0\Pico4SAFTExtTrackingModule.dll`.

#### Compiling VRCFaceTracking.Core

- If you get an error about `vcruntime140.dll`, modify (in the file `VRCFaceTracking\VRCFaceTracking.Core\VRCFaceTracking.Core.csproj`) the reference to `C:\Windows\System32\vcruntime140.dll`
- If you get an error about `fti_osc.dll`, modify (in the file `VRCFaceTracking\VRCFaceTracking.Core\VRCFaceTracking.Core.csproj`) the reference to ? (I just removed it lmao)

## Credits
- [Ben](https://github.com/benaclejames/) for VRCFaceTracking!
- [TofuLemon](https://github.com/ULemon/) with help testing, troubleshooting and providing crucial information that lead to the development of this module!
- [rogermiranda1000](https://github.com/rogermiranda1000) for updating to the latest SA protocol.
