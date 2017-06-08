#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED
namespace RTM.CommandConsole
{
	public class CommandConsoleException : System.ApplicationException
	{
		public CommandConsoleException(string message)
			: base(message)
		{ }

		public CommandConsoleException(string format, params object[] args)
			: base(string.Format(format, args))
		{ }
	}
}
#endif // RTM_CMDCONSOLE_ENABLED