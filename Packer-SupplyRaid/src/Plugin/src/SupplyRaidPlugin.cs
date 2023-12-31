﻿using BepInEx;
using BepInEx.Bootstrap;

namespace SupplyRaid
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	[BepInProcess("h3vr.exe")]
	[BepInDependency("VIP.TommySoucy.H3MP", BepInDependency.DependencyFlags.SoftDependency)]
	public class SupplyRaidPlugin : BaseUnityPlugin
	{
		private readonly Hooks _hooks;
		public static bool h3mpEnabled = false;

		public SupplyRaidPlugin()
		{
			_hooks = new Hooks();
			_hooks.Hook();
		}

		private void Awake()
		{
			h3mpEnabled = Chainloader.PluginInfos.ContainsKey("VIP.TommySoucy.H3MP");
        }

		private void Update()
		{

		}

		private void OnDestroy()
		{
			_hooks.Unhook();
		}
	}
}