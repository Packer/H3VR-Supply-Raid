using BepInEx;

namespace H3VRMod
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		private readonly Hooks _hooks;

		public Plugin()
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