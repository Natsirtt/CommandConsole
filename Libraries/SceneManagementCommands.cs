#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED

using UnityEngine.SceneManagement;

namespace RTM.CommandConsole.Libraries
{
	using Attributes;

	[ConsoleCommandClassCustomizer("Scene")]
	public static class SceneManagmentCommands
	{
		[ConsoleCommand]
		private static string Reload()
		{
			return Load(SceneManager.GetActiveScene().name);
		}

		[ConsoleCommand]
		private static string Load(string sceneName)
		{
			SceneManager.LoadScene(sceneName);
			return string.Format("Loading '{0}'", sceneName);
		}
	}
}

#endif // RTM_CMDCONSOLE_ENABLED