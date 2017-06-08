#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED
using System.Collections.Generic;
using System.Reflection;

namespace RTM.CommandConsole
{
	// TODO - consider if this is something we really want
	// convenience class to represent the KeyValuePair mapping in the dictionary
	// could be moved into CommandWrapper class

	public class CommandDef
	{
		private string _Name = null;
		public string Name {get { return _Name;} }

		private CommandWrapper _Command = null;
		public CommandWrapper Command {get {return _Command; } }

		CommandDef(string name, CommandWrapper command)
		{
			_Name = name;
			_Command = command;
		}

		public static implicit operator CommandDef(KeyValuePair<string, CommandWrapper> pair)
		{
			return new CommandDef(pair.Key, pair.Value);
		}

		public string GetCommandFormat()
		{
			string ret = "";

			var terms = GetCommandFormatAsTerms();
			for(int i=0, num=terms.Length; i<num; i++)
				ret += terms[i] + " ";

			return ret.Trim(new char[] {' '});
		}

		public string[] GetCommandFormatAsTerms()
		{
			return Command.GetCommandFormatAsTerms(Name);
		}

		public bool IsValid()
		{
			return  Name != null &&
					Command != null;

					// TODO - Command.IsValid
		}
	}
}

#endif // RTM_CMDCONSOLE_ENABLED