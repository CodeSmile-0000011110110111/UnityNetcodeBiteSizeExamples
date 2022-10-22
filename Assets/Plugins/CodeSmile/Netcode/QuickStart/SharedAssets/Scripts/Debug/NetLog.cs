// Copyright (C) 2021-2022 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.QuickStart
{
	/// <summary>
	/// Less verbose logging than NetworkLog which also works when the Network is shutdown or not yet initialized.
	/// Adds time ticks to messages: server time ticks + client time tick offset to server time tick, example: 'T[104+9]'
	/// </summary>
	public static class Net
	{
		public static void LogInfo(string message) => LogInternal(message, LogSeverity.Info);
		public static void LogWarning(string message) => LogInternal(message, LogSeverity.Warning);
		public static void LogError(string message) => LogInternal(message, LogSeverity.Error);
		public static void LogInfoServer(string message) => LogInternal(message, LogSeverity.InfoServer);
		public static void LogWarningServer(string message) => LogInternal(message, LogSeverity.WarningServer);
		public static void LogErrorServer(string message) => LogInternal(message, LogSeverity.ErrorServer);

		private static void LogInternal(string message, LogSeverity severity)
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				if (netMan.NetworkTickSystem != null)
				{
					var serverTick = netMan.NetworkTickSystem.ServerTime.Tick;
					var localTick = netMan.NetworkTickSystem.LocalTime.Tick;
					var timeDiff = localTick - serverTick;
					message = timeDiff == 0 ? $"T[{serverTick}] {message}" : $"T[{serverTick}+{timeDiff}] {message}";
				}

				switch (severity)
				{
					case LogSeverity.Info:
						NetworkLog.LogInfo(message);
						break;
					case LogSeverity.InfoServer:
						NetworkLog.LogInfoServer(message);
						break;
					case LogSeverity.Warning:
						NetworkLog.LogWarning(message);
						break;
					case LogSeverity.WarningServer:
						NetworkLog.LogWarningServer(message);
						break;
					case LogSeverity.Error:
						NetworkLog.LogError(message);
						break;
					case LogSeverity.ErrorServer:
						NetworkLog.LogErrorServer(message);
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

		private enum LogSeverity
		{
			Info,
			Warning,
			Error,
			InfoServer,
			WarningServer,
			ErrorServer,
		}
	}
}