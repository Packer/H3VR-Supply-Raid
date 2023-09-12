using BepInEx;
using UnityEngine;

namespace SupplyRaid
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    [BepInDependency("nrgill28.Sodalite", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("nrgill28.Atlas", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("nrgill28.HookGenPatcher_H3VR", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("VIP.H3MP", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("h3vr.exe")]
	public class SupplyRaid : BaseUnityPlugin
	{
		private readonly Hooks _hooks;

		public SupplyRaid()
		{
			_hooks = new Hooks();
			_hooks.Hook();
		}

		private void Awake()
		{

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