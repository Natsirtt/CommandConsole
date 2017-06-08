#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.Assertions;

namespace RTM.CommandConsole
{
	public static class Utils
	{
		private static char[] _CommandDelimeters = new char[]{' '};
		public static char[] CommandDelimeters {get {return _CommandDelimeters; } }

		static Regex DuplicateWhitespaceRegex = new Regex("[ ]{2,}", RegexOptions.None);

		// Helper functions:

		public static string[] GetCommandTerms(string commandStr)
		{
			if(string.IsNullOrEmpty(commandStr))
				return new string[0];

			commandStr = DuplicateWhitespaceRegex.Replace(commandStr, " ");
			return commandStr.Split(CommandDelimeters);
		}

		public static string GetCommand(string[] terms)
		{
			var ret = "";
			for(int i=0, num=terms.Length; i<num; i++)
				ret += terms[i] + " ";

			return ret;
		}

		public static List<Suggestion> FindSuggestionsFor(Type type, string subStr, int max = int.MaxValue)
		{
			Assert.IsNotNull(type);

			if(type.IsSubclassOf(typeof(UnityEngine.Object)))
				return FindUnityObjectSuggestions(type, subStr, max);

			if(type.IsEnum)
				return FindEnumSuggestions(type, subStr, max);
			
			return null;
		}

		private static List<Suggestion> FindUnityObjectSuggestions(Type type, string subStr, int max)
		{
			var ret = new List<Suggestion>();

			var instances = UnityEngine.Object.FindObjectsOfType(type);
			for(int i=0, num=instances.Length; i<num; i++)
			{
				var instance = instances[i];
				if(instance.name.StartsWith(subStr))
					ret.Add(instance.name);
			}
			return ret;
		}

		private static List<Suggestion> FindEnumSuggestions(Type type, string subStr, int max)
		{
			var ret = new List<Suggestion>();

			var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static);
			for(int i=0, num=members.Length; i<num; i++)
			{
				var member = members[i];
				string enumName = member.Name;
				string enumIntVal = ((int)Enum.Parse(type, enumName)).ToString();

				if(enumName.StartsWith(subStr) || enumIntVal.StartsWith(subStr))
				{
					var display = string.Format("{0} ({1})", enumName, enumIntVal);
					ret.Add(new Suggestion(enumName, display));
				}
			}

			return ret;
		}

		public static object ExecuteCommand(ref string commandStr)
		{
			string[] commandTerms = Utils.GetCommandTerms(commandStr);
			if(commandTerms.Length == 0)
				throw(new CommandConsoleException("Empty Command passed '{0}'", commandStr));

			CommandDef commandDef = CommandRegistry.GetCommand(commandTerms[0]);
			if(commandDef == null)
				throw(new CommandConsoleException("Unrecognised Command '{0}'", commandTerms[0]));

			commandStr = FixCommandFormat(commandTerms, commandDef);


			return commandDef.Command.ExecuteCommand(commandTerms);
		}

		static private string FixCommandFormat(string[] commandTerms, CommandDef commandDef)
		{
			string ret = commandDef.Name;

			int numTerms = Mathf.Min(commandTerms.Length, commandDef.Command.GetNumExpectedTerms());
			for(int i=1; i<numTerms; i++)
				ret += " " + commandTerms[i];

			return ret;
		}
	}
}

#endif // RTM_CMDCONSOLE_ENABLED