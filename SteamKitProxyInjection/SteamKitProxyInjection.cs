﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using HarmonyLib;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace SteamKitProxyInjection {
	[Export(typeof(IPlugin))]
	[UsedImplicitly]
	public class SteamKitProxyInjection : IASF {
		public void OnLoaded() {
			ASF.ArchiLogger.LogGenericInfo("Loaded " + Name + " by Vital7 | Support & source code: https://github.com/Vital7/SteamKitProxyInjection");
		}

		public string Name => nameof(SteamKitProxyInjection);
		public Version Version => Assembly.GetExecutingAssembly().GetName().Version ?? throw new InvalidOperationException();

		public void OnASFInit(IReadOnlyDictionary<string, JToken>? additionalConfigProperties = null) {
			ASF.ArchiLogger.LogGenericInfo("Injecting...");
			Harmony harmony = new("tech.Vital7.SteamKitProxyInjection");

			ASF.ArchiLogger.LogGenericTrace("Retrieving WebSocketConnection and WebSocketContext types...");
			Type webSocketConnectionType = AccessTools.TypeByName("SteamKit2.WebSocketConnection");
			Type webSocketContextType = AccessTools.TypeByName("SteamKit2.WebSocketConnection+WebSocketContext");

			ASF.ArchiLogger.LogGenericTrace("Retrieving WebSocketContext constructor...");
			ConstructorInfo constructor = AccessTools.Constructor(webSocketContextType, new[] {webSocketConnectionType, typeof(EndPoint)});
			ASF.ArchiLogger.LogGenericTrace("Patching...");
			harmony.Patch(constructor, postfix: new HarmonyMethod(AccessTools.Method(typeof(SteamKitProxyInjection), nameof(TargetMethod))));
			ASF.ArchiLogger.LogGenericInfo("Successfully injected!");
		}

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public static void TargetMethod(ClientWebSocket ___socket) {
			ASF.ArchiLogger.LogGenericTrace("Retrieving WebProxy config value...");
			IWebProxy? webProxy = ASF.GlobalConfig?.WebProxy;
			if (webProxy == null) {
				return;
			}

			ASF.ArchiLogger.LogGenericTrace("Setting proxy...");
			___socket.Options.Proxy = webProxy;
		}
	}
}
