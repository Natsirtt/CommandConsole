#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

#if RTM_CMDCONSOLE_ENABLED
using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;

#else // !RTM_CMDCONSOLE_ENABLED
using UnityEngine;
#endif // RTM_CMDCONSOLE_ENABLED

namespace RTM.CommandConsole.UI
{
	public class CommandConsoleTextInputHandler : MonoBehaviour
	{
#if RTM_CMDCONSOLE_ENABLED

		private enum EEditMode
		{
			User,
			Auto,
			Clear,
		}

		private enum EBrowseMode
		{
			None,
			History,
			Suggestions,
		}

		[SerializeField]
		private InputField InputText = null;

		[SerializeField]
		private Text HistoryText = null;

		[SerializeField]
		private Text SuggestionText = null;

		private EEditMode EditMode;
		private string UserInput = "";
		private string[] UserInputTerms = new string[0];
		
		private List<string> CommandHistory = new List<string>();
		private List<Suggestion> Suggestions = new List<Suggestion>();

		private EBrowseMode BrowseMode = EBrowseMode.None;
		private int BrowseIdx = -1;
		
		private EventSystem GetEventSystem()
		{
			if(!EventSystem.current)
				CommandConsoleUIBootStrap.CreateEventSystem();

			return EventSystem.current;
		}

		void Start()
		{
			if(InputText == null)
				InputText = GetComponent<InputField>();
			Assert.IsNotNull(InputText);

			InputText.onValueChanged.AddListener(OnValueChanged);
			InputText.onEndEdit.AddListener(OnEndEdit);

			ClearHistory();
			ClearInputText();
			ClearSuggestions();
		}

		void Update()
		{
			Assert.IsTrue(EditMode == EEditMode.User);

			if(!InputText.isFocused)
				SetFocus();

			UpdateBrowseInput();
		}

		void OnEnable()
		{
			ClearInputText();
			ClearSuggestions();
			SetFocus();
			ResetBrowse();

			EditMode = EEditMode.User;
		}

		public void OnValueChanged(string value)
		{
			if(EditMode == EEditMode.User || EditMode == EEditMode.Clear)
			{
				UserInput = value;
				UserInputTerms = Utils.GetCommandTerms(value);
				ResetBrowse();
			}

			UpdateSuggestions();
			EditMode = EEditMode.User;
		}

		public void OnEndEdit(string value)
		{
			bool isSubmit = Input.GetKeyDown(KeyCode.KeypadEnter) ||
							Input.GetKeyDown(KeyCode.Return);
							
			if(isSubmit && InputText.text.Length > 0)
				DoSubmit();
		}

		private void DoSubmit()
		{
			string commandStr = InputText.text;

			object response = null;
			try
			{
				response = Utils.ExecuteCommand(ref commandStr);			// Note to self - need to execute before outputting as the command needs sanitizing in Execute command
			}
			catch(CommandConsoleException e)
			{
				response = e.Message;
			}
			catch(Exception e)
			{
				response = "Command generated internal exception: " + e.Message;
			}

			Output("\n> " + commandStr.ToString());
					
			if(response != null)
				Output("\n" + response);
			Output("\n");

			ClearInputText();
			ResetBrowse();

			CommandHistory.Insert(0, commandStr);
		}

		private void SetFocus()
		{
			//if(!EventSystem.current.currentSelectedGameObject == gameObject)
			{
				GetEventSystem().SetSelectedGameObject(gameObject, null);
				InputText.OnPointerClick(new PointerEventData(EventSystem.current));
			}
		}

		void UpdateSuggestions()
		{
			ClearSuggestions();
			if(BrowseMode == EBrowseMode.History)
				return;
			
			int termIdx = UserInputTerms.Length - 1;
			if(termIdx == 0)
				DoCommandSuggestions();
			else if(termIdx > 0)
				DoParameterSuggestions(termIdx);
		}

		private void DoCommandSuggestions()
		{
			// TODO - null ptr checks for suggestion text box

			Dictionary<string, CommandWrapper> suggestions = CommandRegistry.GetCommandsLike(UserInputTerms[0]);
			if(suggestions == null || suggestions.Count == 0)
			{
				SuggestionText.text = "No Matching Commands!";
				return;
			}

			foreach(CommandDef commandDef in suggestions)
			{
				Suggestions.Add(commandDef.Name);
				SuggestionText.text += commandDef.GetCommandFormat() + "\n";
			}
		}

		private void DoParameterSuggestions(int termIdx)
		{
			// TODO - null ptr checks for suggestion text box

			CommandDef commandDef = CommandRegistry.GetCommand(UserInputTerms[0]);
			if(commandDef == null)
			{
				SuggestionText.text = "No Matching Commands!";
				return;
			}
			Assert.IsTrue(commandDef.IsValid());

			if(termIdx < 0 || termIdx >= commandDef.Command.GetNumExpectedTerms())
				return;
				
			string[] terms  = commandDef.GetCommandFormatAsTerms();
			if(termIdx < terms.Length)
				terms[termIdx] = "<b><color=#ffffffff>" + terms[termIdx] + "</color></b>";

			SuggestionText.text += Utils.GetCommand(terms) + "\n";

			string searchTerm = termIdx < UserInputTerms.Length ? UserInputTerms[termIdx] : "";
			var paramSuggestions = Utils.FindSuggestionsFor(commandDef.Command.GetParamType(termIdx-1), searchTerm);

			if(paramSuggestions != null )
			{
				string suggestionDisplayPrefix = "";
				for(int i=0; i < Mathf.Min(terms.Length, termIdx); i++)
					suggestionDisplayPrefix += terms[i] + " ";

				suggestionDisplayPrefix = "<color=#ffffff00>" + suggestionDisplayPrefix + "</color>";

				for(int i=0, num=paramSuggestions.Count; i<num; i++)
				{
					var paramSuggestion = paramSuggestions[i];
					Suggestions.Add(paramSuggestion);
					SuggestionText.text += suggestionDisplayPrefix + paramSuggestion.display + "\n";
				}
			}
		}

		private void UpdateBrowseInput()
		{
			if(Input.GetKeyDown(KeyCode.UpArrow))
			{
				Browse(1);
			}
			else if(Input.GetKeyDown(KeyCode.DownArrow))
			{
				Browse(-1);
			}
			else if(Input.GetKeyDown(KeyCode.Tab))
			{
				if(Input.GetKey(KeyCode.LeftShift))
				{
					if(BrowseMode == EBrowseMode.Suggestions)
						BrowseSuggestions(-1);
				}
				else
				{
					if(BrowseMode == EBrowseMode.None || BrowseMode == EBrowseMode.Suggestions)
						BrowseSuggestions(1);
				}
			}
		}

		private void Browse(int move)
		{	
			if(BrowseMode == EBrowseMode.History || BrowseMode == EBrowseMode.None && move > 0)
				BrowseHistory(move);
			else if(BrowseMode == EBrowseMode.Suggestions || BrowseMode == EBrowseMode.None && move < 0)
				BrowseSuggestions(-move);
		}

		private void ResetBrowse()
		{
			BrowseMode = EBrowseMode.None;
			BrowseIdx = -1;
			UpdateSuggestions();
		}

		private void BrowseHistory(int move)
		{
			int oldBrowseIdx = BrowseIdx;
			BrowseIdx = Mathf.Clamp(BrowseIdx + move, -1, CommandHistory.Count - 1);
			if(BrowseIdx != oldBrowseIdx)
			{
				if(BrowseIdx < 0)
				{
					SetInputText(UserInput, EEditMode.Auto);
					ResetBrowse();
				}
				else
				{
					BrowseMode = EBrowseMode.History;
					SetInputText(CommandHistory[BrowseIdx], EEditMode.Auto);
				}
			}
		}

		private void BrowseSuggestions(int move)
		{
			int oldBrowseIdx = BrowseIdx;
			BrowseIdx = Mathf.Clamp(BrowseIdx + move, -1, Suggestions.Count - 1);
			if(BrowseIdx != oldBrowseIdx)
			{
				if(BrowseIdx < 0)
				{
					SetInputText(UserInput, EEditMode.Auto);
					ResetBrowse();
				}
				else
				{
					BrowseMode = EBrowseMode.Suggestions;
					SetInputText(ExpandSuggestion(BrowseIdx), EEditMode.Auto);
				}
			}
		}

		private string ExpandSuggestion(int suggestionIdx)
		{
			Assert.IsTrue(suggestionIdx >= 0 && suggestionIdx < Suggestions.Count);
			string output = "";
			for(int i=0; i<UserInputTerms.Length-1; i++)
				output+= UserInputTerms[i] + " ";

			return output + Suggestions[suggestionIdx].value;
		}

		//////////////////////////////////////////////////////////////////////
		// Update text box helper functions

		private void SetInputText(string str, EEditMode editMode)
		{
			if(InputText && str != InputText.text)
			{
				EditMode = editMode;
				InputText.text = str;
				InputText.caretPosition = int.MaxValue;
			}
		}

		private void ClearInputText()
		{
			SetInputText("", EEditMode.Clear);
		}

		private void ClearSuggestions()
		{
			if(SuggestionText)
				SuggestionText.text = "";

			Suggestions = new List<Suggestion>();
		}

		public void ClearHistory()
		{
			if(HistoryText)
				HistoryText.text = "";

			CommandHistory.Clear();
		}

		private void Output(string value)
		{
			if(HistoryText != null)
				HistoryText.text += value;
			else
				Debug.Log(value);
		}

#else // !RTM_CMDCONSOLE_ENABLED
		void Awake()
		{
			GameObject.Destroy(gameObject);
		}	
#endif // RTM_CMDCONSOLE_ENABLED
	}
}