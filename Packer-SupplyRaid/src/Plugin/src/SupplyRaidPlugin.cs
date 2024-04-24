using BepInEx;
using BepInEx.Bootstrap;
using Atlas;
using Atlas.Loaders;

namespace SupplyRaid
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	[BepInProcess("h3vr.exe")]
	[BepInDependency("VIP.TommySoucy.H3MP", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("dll.potatoes.ptnhbgml", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AtlasConstants.Guid, AtlasConstants.Version)]
    public class SupplyRaidPlugin : BaseUnityPlugin
	{
		private readonly Hooks _hooks;
		public static bool h3mpEnabled = false;
		public static bool bgmEnabled = false;

		public SupplyRaidPlugin()
		{
			_hooks = new Hooks();
			_hooks.Hook();
		}

		private void Awake()
		{
            AtlasPlugin.Loaders["supplyraid"] = new SandboxLoader();
            h3mpEnabled = Chainloader.PluginInfos.ContainsKey("VIP.TommySoucy.H3MP");
			bgmEnabled = Chainloader.PluginInfos.ContainsKey("dll.potatoes.ptnhbgml");
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