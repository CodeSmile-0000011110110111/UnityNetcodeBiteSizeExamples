// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.BiteSize
{
	public static class Net
	{
		private enum LogSeverity
		{
			Info,
			Warning,
			Error,
			InfoServer,
			WarningServer,
			ErrorServer,
		}

		public static void LogInfo(string message) => LogInternal("", LogSeverity.Info);
		public static void LogWarning(string message) => LogInternal("", LogSeverity.Warning);
		public static void LogError(string message) => LogInternal("", LogSeverity.Error);
		public static void LogInfoServer(string message) => LogInternal("", LogSeverity.InfoServer);
		public static void LogWarningServer(string message) => LogInternal("", LogSeverity.WarningServer);
		public static void LogErrorServer(string message) => LogInternal("", LogSeverity.ErrorServer);

		private static void LogInternal(string message, LogSeverity severity)
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				var serverTime = netMan.NetworkTickSystem.ServerTime;
				var localTime = netMan.NetworkTickSystem.LocalTime;
				var log = $"T[s:{serverTime} l:{localTime}] {message}";
				switch (severity)
				{
					case LogSeverity.Info:
						NetworkLog.LogInfo(log);
						break;
					case LogSeverity.InfoServer:
						NetworkLog.LogInfoServer(log);
						break;
					case LogSeverity.Warning:
						NetworkLog.LogWarning(log);
						break;
					case LogSeverity.WarningServer:
						NetworkLog.LogWarningServer(log);
						break;
					case LogSeverity.Error:
						NetworkLog.LogError(log);
						break;
					case LogSeverity.ErrorServer:
						NetworkLog.LogErrorServer(log);
						break;
				}
			}
			else
			{
				switch (severity)
				{
					case LogSeverity.Info:
					case LogSeverity.InfoServer:
						Debug.Log(message);
						break;
					case LogSeverity.Warning:
					case LogSeverity.WarningServer:
						Debug.LogWarning(message);
						break;
					case LogSeverity.Error:
					case LogSeverity.ErrorServer:
						Debug.LogError(message);
						break;
				}
			}
		}
	}
}