using BepInEx;

namespace SupplyRaid
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	[BepInProcess("h3vr.exe")]
	public class SupplyRaidPlugin : BaseUnityPlugin
	{
		private readonly Hooks _hooks;

		public SupplyRaidPlugin()
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