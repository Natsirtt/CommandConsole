#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED

using UnityEngine;

namespace RTM.CommandConsole.UI
{
	class CommandConsoleUIBootStrap
	{
		const string CanvasPrefabName = "CommandConsoleCanvas";
		const string EventSystemPrefabName = "CommandConsoleEventSystem";

		[RuntimeInitializeOnLoadMethod]
		private static void CreateUI()
		{
			var canvas = Instantiate(CanvasPrefabName);
			if(canvas)
				Object.DontDestroyOnLoad(canvas);
		}

		public static void CreateEventSystem()
		{
			Instantiate(EventSystemPrefabName);
		}

		private static Object Instantiate(string resourceName)
		{
			var prefab = Resources.Load(resourceName);
			if(prefab)
			{
				var go = GameObject.Instantiate(prefab);
				if(go)
					go.name = resourceName;
				return go;
			}
			return null;
		}
	}
}
#endif // RTM_CMDCONSOLE_ENABLED