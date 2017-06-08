#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED

namespace RTM.CommandConsole
{
	public struct Suggestion
	{
		public string value;
		public string display;

		public Suggestion(string value, string display)
		{
			this.value = value;
			this.display = display;
		}

		public static implicit operator Suggestion(string value)
		{
			return new Suggestion(value, value);
		}
	}
}

#endif // RTM_CMDCONSOLE_ENABLED