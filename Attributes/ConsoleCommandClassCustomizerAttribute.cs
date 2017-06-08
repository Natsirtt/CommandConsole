using System;

namespace RTM.CommandConsole.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConsoleCommandClassCustomizerAttribute : Attribute
	{
		public ConsoleCommandClassCustomizerAttribute(string customName)
		{
			_customName = customName;
		}

		private string _customName = null;
		public string CustomName {get { return _customName;} }
	}
}