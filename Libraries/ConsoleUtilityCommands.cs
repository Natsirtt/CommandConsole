#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED

namespace RTM.CommandConsole.Libraries
{
	using Attributes;

	[ConsoleCommandClassCustomizer("")]
	public static class ConsoleUtilityCommands
	{
		[ConsoleCommand]
		private static string Echo(string input)
		{
			return input;
		}

		[ConsoleCommand]
		private static void ClearHistory()
		{
			var textHandlers = UnityEngine.GameObject.FindObjectsOfType<UI.CommandConsoleTextInputHandler>();
			for(int i=0, num=textHandlers.Length; i<num; i++)
				textHandlers[i].ClearHistory();
		}

		[ConsoleCommand]
		private static string PrintCommands()
		{
			string ret = "";

			foreach(CommandDef command in CommandRegistry.GetCommands())
				ret += command.GetCommandFormat() + "\n";
			
			return ret;
		}
	}
}

#endif // RTM_CMDCONSOLE_ENABLED