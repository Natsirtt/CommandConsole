#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED

using UnityEngine;
using UnityEngine.Assertions;

namespace RTM.CommandConsole
{
	public class Settings : ScriptableObject
	{
		[Header("Control Keys")]
		[SerializeField]
		private KeyCode _OpenConsoleKey = KeyCode.KeypadDivide;
		public static KeyCode OpenConsoleKey { get { return Instance._OpenConsoleKey; } }

		[SerializeField]
		private KeyCode _CloseConsoleKey = KeyCode.KeypadDivide;
		public static KeyCode CloseConsoleKey { get { return Instance._CloseConsoleKey; } }


		/////////////////////////////////////////////////////////////////////////////////////////
		// Project Settings implementation
		// Could be inherited from ScriptableObjectSingleton, but would cause some conflicts
		// TODO - consider duplicating the ScriptableObjectSingleton with namespace fallbacks

		const string AssetName = "CommandConsoleSettings";

		static private Settings _Instance = null;
		static public Settings Instance { get { return GetOrCreateInstance(); } }

		static public Settings GetOrCreateInstance()
		{
			if(_Instance)
				return _Instance;

			_Instance = Resources.Load<Settings>(AssetName);

			if(!_Instance)
			{
			var asset = ScriptableObject.CreateInstance<Settings>();
#if false
			UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/" + AssetName + ".asset");
#endif
			_Instance = asset;
			}

			Assert.IsNotNull(_Instance);
			return _Instance;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Edit/Project Settings/RTM Helpers/Command Console Settings")]
		private static void OpenAsset()
		{
			UnityEditor.Selection.activeObject = Instance;
		}
#endif

	}
}

#endif // RTM_CMDCONSOLE_ENABLED