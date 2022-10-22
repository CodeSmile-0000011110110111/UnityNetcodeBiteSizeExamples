// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Unity.Netcode;

namespace CodeSmile.Netcode.QuickStart
{
	public static class NetcodeUtils
	{
		private static readonly ulong[] ClientId = new ulong[1];

		public static ClientRpcParams SendTo(ServerRpcParams rpcParams)
		{
			ClientId[0] = rpcParams.Receive.SenderClientId;
			return new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = ClientId } };
		}

		public static ClientRpcParams SendTo(ulong clientId)
		{
			ClientId[0] = clientId;
			return new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = ClientId } };
		}

		public static string GetSHA1Hash(IReadOnlyList<byte> data)
		{
			using (var sha1 = new SHA1CryptoServiceProvider())
				return string.Concat(sha1.ComputeHash(data.ToArray()).Select(x => x.ToString("X2")));
		}

		public static string GetSaltedSHA1Hash(string text, string salt = "v;r+?H+VNß]&l0w=Wtb:L3vu&8ppwt}0")
		{
			if (string.IsNullOrEmpty(text))
				return null;

			return GetSHA1Hash(Encoding.UTF8.GetBytes(salt + text));
		}

		public static string GetPublicIPv4()
		{
			var ipString = new WebClient().DownloadString("http://icanhazip.com");
			return ipString.Replace("\\r\\n", "").Replace("\\n", "").Trim();
		}

		public static string[] GetLocalIPv4(NetworkInterfaceType interfaceType = NetworkInterfaceType.Ethernet)
		{
			var ipAddrList = new List<string>();
			foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (netInterface.NetworkInterfaceType == interfaceType && netInterface.OperationalStatus == OperationalStatus.Up)
				{
					foreach (var ip in netInterface.GetIPProperties().UnicastAddresses)
					{
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
							ipAddrList.Add(ip.Address.ToString());
					}
				}
			}
			return ipAddrList.ToArray();
		}

		public static string GetFirstLocalIPv4(NetworkInterfaceType interfaceType = NetworkInterfaceType.Ethernet) =>
			GetLocalIPv4(interfaceType)?.FirstOrDefault();

		public static IPAddress[] ResolveHostname(string hostName, bool ip6Wanted = false)
		{
			// hostname may already be an IP address
			if (IPAddress.TryParse(hostName, out var outIpAddress))
				return new[] { outIpAddress };

			try
			{
				var addresslist = Dns.GetHostAddresses(hostName);
				if (addresslist == null || addresslist.Length == 0)
					return new IPAddress[0];

				if (ip6Wanted)
					return addresslist.Where(o => o.AddressFamily == AddressFamily.InterNetworkV6).ToArray();

				return addresslist.Where(o => o.AddressFamily == AddressFamily.InterNetwork).ToArray();
			}
			catch {}

			return null;
		}

		public static string TryResolveHostname(string hostName, bool ip6Wanted = false) =>
			ResolveHostname(hostName, ip6Wanted)?.FirstOrDefault()?.ToString();
	}
}