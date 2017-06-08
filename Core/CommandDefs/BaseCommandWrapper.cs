#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED
using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine.Assertions;

namespace RTM.CommandConsole
{
	public abstract class CommandWrapper
	{
		public abstract MemberInfo Member {get; }
		public abstract bool IsStatic();
		public abstract int GetNumParams();
		public abstract Type GetParamType(int paramIdx);
		public abstract string[] GetCommandFormatAsTerms(string commandName);

		protected abstract object[] ProcessParameters(string[] commandTerms);
		protected abstract object ExecuteCommandImpl(object instance, object[] parameters);

		public int GetNumExpectedTerms()
		{
			return GetNumParams() + 1;
		}

		protected int GetParamStartTermIdx()
		{
			return IsStatic() ? 1 : 2;
		}

		public object ExecuteCommand(string[] terms)
		{
			terms = Preprocess(terms);

			object instance = null;
			if(!IsStatic() && terms.Length > 1)
				instance = FindInstance(terms[1]);

			object[] parameters = ProcessParameters(terms);

			return ExecuteCommandImpl(instance, parameters);
		}

		private string[] Preprocess(string[] terms)
		{
			var numTerms = terms.Length;
			var ret = new List<string>(numTerms);
			for(int i=0; i<numTerms; i++)
			{
				Assert.IsNotNull(terms[i]);
				if(terms[i] == "")
				{
					Assert.IsTrue(i == numTerms-1);		// only the last term can be an empty string
					break;
				}

				ret.Add(terms[i]);
			}

			return ret.ToArray();
		}

		private object FindInstance(string instanceName)
		{
			if(IsStatic() || string.IsNullOrEmpty(instanceName))
				return null;

			UnityEngine.Object[] candidates = UnityEngine.GameObject.FindObjectsOfType(Member.DeclaringType);
			for(int i=0, num=candidates.Length; i<num; i++)
			{
				var candidate = candidates[i];
				if(candidate.name == instanceName)
					return candidate;
			}

			return null;
		}
	}
}
#endif // RTM_CMDCONSOLE_ENABLED