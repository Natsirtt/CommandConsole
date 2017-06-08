#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

namespace RTM.CommandConsole
{
	using Attributes;

	public class StringComparerIgnoreCase : IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			if (x != null && y != null)
			{
				return x.ToLowerInvariant() == y.ToLowerInvariant();
			}
			return false;
		}

		public int GetHashCode(string obj)
		{
			return obj.ToLowerInvariant().GetHashCode();
		}
	}

	public static class CommandRegistry
	{
		private static Dictionary<string, CommandWrapper> Commands = new Dictionary<string, CommandWrapper>(new StringComparerIgnoreCase());

		[RuntimeInitializeOnLoadMethod]
		private static void Init(){}

		static CommandRegistry()
		{
			BindingFlags bindings = BindingFlags.Public |
									BindingFlags.NonPublic | 
									BindingFlags.Static |
									BindingFlags.Instance;

			Assembly assembly = typeof(CommandRegistry).Assembly;
			Assert.IsNotNull(assembly);

			var types = assembly.GetTypes();
			for(int i=0, num=types.Length; i<num; i++)
			{
				var type = types[i];
				string className = GetClassName(type);

				var methodInfos = type.GetMethods(bindings);
				for(int j=0, jNum=methodInfos.Length; j<jNum; j++)
				{
					var methodInfo = methodInfos[j];
					var attribute = GetCommandAttribute(methodInfo);
					if(attribute != null)
						AddCommand(new MethodCommandWrapper(methodInfo), attribute, className);
				}

				var fieldInfos = type.GetFields(bindings);
				for(int j=0, jNum=fieldInfos.Length; j<jNum; j++)
				{
					var fieldInfo=fieldInfos[j];
					var attribute = GetCommandAttribute(fieldInfo);
					if(attribute != null)
						AddCommand(new FieldCommandWrapper(fieldInfo), attribute, className);
				}
				
				var propertyInfos = type.GetProperties(bindings);
				for(int j=0, jNum=propertyInfos.Length; j<jNum; j++)
				{
					var propertyInfo = propertyInfos[j];
					var attribute = GetCommandAttribute(propertyInfo);
					if(attribute != null)
						AddCommand(new PropertyCommandWrapper(propertyInfo), attribute, className);
				}
			}
		}

		private static ConsoleCommandAttribute GetCommandAttribute(MemberInfo memberInfo)
		{
			object[] attributes = memberInfo.GetCustomAttributes(typeof(ConsoleCommandAttribute), true);

			return attributes.Length > 0 ? attributes[0] as ConsoleCommandAttribute : null;
		}

		private static void AddCommand(CommandWrapper CommandWrapper, ConsoleCommandAttribute attribute, string prefix)
		{
			Assert.IsNotNull(CommandWrapper.Member);
			Assert.IsNotNull(attribute);
			Assert.IsNotNull(prefix);

			if(!CommandWrapper.IsStatic() && !(CommandWrapper.Member.DeclaringType.IsSubclassOf(typeof(MonoBehaviour))))
				return;
					
			string methodName = GetCommandName(CommandWrapper.Member, attribute);

			Assert.IsFalse(String.IsNullOrEmpty(methodName));

			string commandString = prefix.Length > 0 ? prefix + '.' + methodName : methodName;
			Commands.Add(commandString, CommandWrapper);
		}

		private static string GetClassName(Type type)
		{
			String className = type.Name;
			object[] classAttributes = type.GetCustomAttributes(typeof(ConsoleCommandClassCustomizerAttribute), false);
			if(classAttributes.Length > 0)
			{
				ConsoleCommandClassCustomizerAttribute classAttribute = classAttributes[0] as ConsoleCommandClassCustomizerAttribute;
				if(classAttribute != null)
					return classAttribute.CustomName;
			}
			return type.ToString();
		}

		private static string GetCommandName(MemberInfo memberInfo, ConsoleCommandAttribute attribute)
		{
			string customMethodName = attribute == null ? "" : attribute.CustomName;
			return String.IsNullOrEmpty(customMethodName) ? memberInfo.Name : customMethodName;
		}

		public static Dictionary<string, CommandWrapper> GetCommands()
		{
			// shallow copy should be fine to prevent accidental messing with registry
			return new Dictionary<string, CommandWrapper>(Commands, new StringComparerIgnoreCase());
		}

		public static MethodInfo GetCommandMethodInfo(string commandString)
		{
			CommandWrapper ret = null;
			Commands.TryGetValue(commandString, out ret);
			return ret.Member as MethodInfo;										// TODO - fix
		}

		public static CommandDef GetCommand(string commandString)
		{
			foreach(CommandDef commandDef in Commands)
			{
				if(commandDef.Name.Equals(commandString, StringComparison.InvariantCultureIgnoreCase))
					return commandDef;
			}
			return null;
		}

		public static Dictionary<string, CommandWrapper> GetCommandsLike(string inString)
		{
			string[] split = inString.Split(Utils.CommandDelimeters, 2);
			string commandString = split.Length > 0 ? split[0] : null;

			if(String.IsNullOrEmpty(commandString))
				return null;

			bool partialCommand = split.Length == 1;

			Dictionary<string, CommandWrapper> similar = new Dictionary<string, CommandWrapper>();
			foreach(CommandDef commandDef in Commands)
			{
				if(partialCommand)
				{
					if(commandDef.Name.StartsWith(commandString, StringComparison.InvariantCultureIgnoreCase))
						similar.Add(commandDef.Name, commandDef.Command);
				}
				else
				{
					if(commandDef.Name.Equals(commandString, StringComparison.InvariantCultureIgnoreCase))
					{
						similar.Add(commandDef.Name, commandDef.Command);
						break;
					}
				}
			}
			return similar;
		}
	}
}

#endif // RTM_CMDCONSOLE_ENABLED