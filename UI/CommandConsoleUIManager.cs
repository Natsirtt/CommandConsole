#if(UNITY_EDITOR || DEVELOPMENT_BUILD || RTM_CMDCONSOLE_RELEASE)
#define RTM_CMDCONSOLE_ENABLED
#endif

using UnityEngine;

namespace RTM.CommandConsole.UI
{
	public class CommandConsoleUIManager : MonoBehaviour
	{
#if RTM_CMDCONSOLE_ENABLED

		[Header("Object References")]
		[SerializeField]
		private GameObject ConsoleDisplay = null;
	
		void Start()
		{
			if(ConsoleDisplay != null)
				ConsoleDisplay.SetActive(false);
		}

		void Update()
		{
			if(ConsoleDisplay == null)
				return;

			bool active = ConsoleDisplay.activeInHierarchy;
			if(!active && Input.GetKeyDown(Settings.OpenConsoleKey))
				ConsoleDisplay.SetActive(true);
			else if(active && Input.GetKeyDown(Settings.CloseConsoleKey))
				ConsoleDisplay.SetActive(false);
		}
#else // !RTM_CMDCONSOLE_ENABLED
		void Awake()
		{
			GameObject.Destroy(gameObject);
		}
		
#endif //RTM_CMDCONSOLE_ENABLED
	}
}