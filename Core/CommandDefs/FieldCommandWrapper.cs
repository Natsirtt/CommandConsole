#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED
using System;
using System.Reflection;

using UnityEngine.Assertions;

namespace RTM.CommandConsole
{
	public class FieldCommandWrapper : CommandWrapper
	{
		private FieldInfo _Field = null;
		private FieldInfo Field {get {return _Field; } }

		public FieldCommandWrapper(FieldInfo field)
		{
			_Field = field;
		}

		public override MemberInfo Member {get {return _Field;} }

		public override bool IsStatic() 
		{
			return _Field.IsStatic;
		}

		public override string[] GetCommandFormatAsTerms(string commandName)
		{
			var terms = new string[GetNumExpectedTerms()];
			int termIdx = 0;

			terms[termIdx++] = commandName;

			if(!IsStatic())
				terms[termIdx++] = "Instance(" + _Field.DeclaringType.Name + ") ";

			terms[termIdx++] = "NewValue(" + _Field.FieldType.Name + ")";

			return terms;
		}

		public override int GetNumParams()
		{
			return IsStatic() ? 1 : 2;
		}

		public override Type GetParamType(int paramIdx)
		{
			Assert.IsNotNull(_Field);
			Assert.IsTrue(paramIdx >= 0 && paramIdx < GetNumParams() );

			if(!IsStatic() && paramIdx == 0)
				return _Field.DeclaringType;

			return _Field.FieldType;
		}

		protected override object ExecuteCommandImpl(object instance, object[] parameters)
		{
			if(parameters != null && parameters.Length > 0)
				_Field.SetValue(instance, parameters[0]);

			return _Field.GetValue(instance);
		}

		protected override object[] ProcessParameters(string[] commandTerms)
		{
			Assert.IsNotNull(commandTerms);

			int paramIdx = GetParamStartTermIdx();
			if(commandTerms.Length > paramIdx)
				return new object[]{StringTypeConverter.ToType(commandTerms[paramIdx], _Field.FieldType) };
			else
				return new object[0];
		}
	}
}
#endif // RTM_CMDCONSOLE_ENABLED