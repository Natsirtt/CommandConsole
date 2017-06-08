#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED

using System;
using UnityEngine;

namespace RTM.CommandConsole
{
	public static class StringTypeConverter
	{
		public static object ToType(string inString, Type type)
		{
			try
			{
				return Convert.ChangeType(inString, type);
			}
			catch(Exception e)
			{
				if(e is InvalidCastException)
				{
					// TODO - find a more automated way of handling this

					if(type == typeof(UnityEngine.Object) || type.IsSubclassOf(typeof(UnityEngine.Object)))
						return ToUnityObject(type, inString);

					if(type.IsEnum)
						return Enum.Parse(type, inString);

					if(type == typeof(Vector3))
						return ToVector3(inString);

					if(type == typeof(Vector2))
						return ToVector2(inString);
				}
				else if(e is FormatException || e is OverflowException || e is ArgumentNullException)
				{
					return Activator.CreateInstance(type);
				}
				else
				{
					throw e;
				}
			}
			return null;
		}

		public static T ToType<T>(string inStr)
		{
			return (T)ToType(inStr, typeof(T));
		}

		public static UnityEngine.Object ToUnityObject(Type type, string inStr)
		{
			var objects = GameObject.FindObjectsOfType(type);
			for(int i=0, num=objects.Length; i<num; i++)
			{
				var obj = objects[i];
				if(obj.name == inStr)
					return obj;
			}

			return null;
		}

		public static Vector3 ToVector3(string inStr)
		{
			string[] components = TrimAndSplit(inStr);

			int numComps = components.Length;
			return new Vector3(
					numComps > 0 ? float.Parse(components[0]) : 0.0f, 
					numComps > 1 ? float.Parse(components[1]) : 0.0f,
					numComps > 2 ? float.Parse(components[2]) : 0.0f);
		}

		public static Vector2 ToVector2(string inStr)
		{
			string[] components = TrimAndSplit(inStr);

			int numComps = components.Length;
			return new Vector2(
					numComps > 0 ? float.Parse(components[0]) : 0.0f, 
					numComps > 1 ? float.Parse(components[1]) : 0.0f);
		}

		public static string[] TrimAndSplit(string inString)
		{
			char[] trimmers = new char[]{',', ' '};
			char[] separators = new char[]{','};

			return inString.Trim(trimmers).Split(separators);
		}
	}
}

#endif // RTM_CMDCONSOLE_ENABLED