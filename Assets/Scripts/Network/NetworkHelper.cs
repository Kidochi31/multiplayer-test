using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

public static class NetworkHelper
{
    readonly static (string Host, int Port)[] stunHosts =
    new (string Host, int Port)[]{
        ("stun.l.google.com", 19302),
        ("stun1.l.google.com", 19302),
        ("stun2.l.google.com", 19302),
        ("stun3.l.google.com", 19302),
        ("stun4.l.google.com", 19302),

        ("stun.cloudflare.com", 3478),

        ("stun.nextcloud.com", 443),

        ("stun.sipgate.net", 3478),

        ("stun.ekiga.net", 3478)
    };
    public static List<IPEndPoint> StunHosts = new();

    static NetworkHelper()
    {
        
        foreach (var (host, port) in stunHosts)
        {
            try{
                foreach (IPAddress ip in Dns.GetHostAddresses(host))
                {
                    StunHosts.Add(new IPEndPoint(ip, port));
                }
                } catch (Exception){}
        }
        

    }

    public static IPAddress? GetLocalIPv4()
    {
        foreach(NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if(nic.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            foreach(UnicastIPAddressInformation address in nic.GetIPProperties().UnicastAddresses)
            {
                if(address.Address.AddressFamily == AddressFamily.InterNetwork
                    && !IPAddress.IsLoopback(address.Address))
                {
                    return address.Address;
                }
            }
        }

        return null;
    }

    public static IPEndPoint? GetIPEndPointFromText(string input)
    {
        input = input.Trim();
        if (!input.Contains(':'))
        {
            return null;
        }
        string[] split = input.Split(':');
        if(split.Length != 2)
        {
            return null;
        }
        string possibleIp = split[0];
        possibleIp = possibleIp.Trim();
        bool isIp = IPAddress.TryParse(possibleIp, out IPAddress? ip);
        if (!isIp || ip is null)
        {
            return null;
        }
        string possiblePort = split[1];
        possiblePort = possiblePort.Trim();
        bool isPort = int.TryParse(possiblePort, out int port);
        if (!isPort)
        {
            return null;
        }
        try{
            return new IPEndPoint(ip, port);
        } catch (Exception)
        {
            return null;
        }
    }
}
