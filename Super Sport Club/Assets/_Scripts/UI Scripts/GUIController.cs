using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GUIController: MonoBehaviour
{
	public float timeSinceService;
	public IUIState UIState 
	{
		get{ return uiState; }
		set 
		{
			if (uiState != null)
			{
				uiState.ExitState ();
			}
			uiState = value;
			uiState.EnterState ();
		}
	}
	public UIMainMenu UIMM;
	public UISetPiece UISP;
	public UIGameHUD UIHUD;
	[SerializeField] string AppId;// set in inspector. this is called when the client loaded and is ready to start
	[SerializeField] float serviceInterval = 1;
	[SerializeField] LayerMask mask;
	[SerializeField] Image panelFab, characterCardFab;
	[SerializeField] SpriteRenderer meterFab;
	[SerializeField] Button buttFab, NewGame;
	[SerializeField] Text infoText;
	[SerializeField] Canvas UIcan;
	[SerializeField] RectTransform MainMenu, CharacterPanel;
	private IUIState uiState;
	private CustomGameClient GameClientInstance;
	private Grid_Setup board;

	void Awake()
	{
		this.board = GetComponent<Grid_Setup>();
		Grid_Setup.Instance = this.board;
		this.GameClientInstance  = new CustomGameClient();
		this.GameClientInstance.AppId = AppId;  // edit this!
		this.GameClientInstance.board = board;
		this.GameClientInstance.gui = this;

		Application.runInBackground = true;
		CustomTypes.Register();

		UIMM = new UIMainMenu(this,ref GameClientInstance);
		UIMM.SetFabs (MainMenu, NewGame);
		UISP = new UISetPiece (this,ref GameClientInstance);
		UISP.SetFabs (CharacterPanel, characterCardFab, infoText);
		UIHUD = new UIGameHUD(this,ref GameClientInstance);
		UIHUD.SetFabs (UIcan, panelFab,buttFab,meterFab, infoText);
		UIState = UIMM;
	}
	
	void Update()
	{
		timeSinceService += Time.deltaTime;
		if (timeSinceService > serviceInterval)
		{
			this.GameClientInstance.Service();
			timeSinceService = 0;
		}
		UIState.Update ();
	}
	void Start()
	{
		StartCoroutine ("UpdateInfo");
	}
	public IEnumerator UpdateInfo ()
	{
		while (true) 
		{
			string side =  (int)GameClientInstance.team>0 ? "Right":"Left" ;
			infoText.text = string.Format("team: {0}. \n You're on the: {1} side. \n Turn: {2}", (int)GameClientInstance.team , side, GameClientInstance.TurnNumber);
			infoText.text += string.Format("Opponenent ready: {0}", GameClientInstance.HasOppSubmitted());
			yield return new WaitForSeconds (1f);
		}
		yield return null;
	}

	void FixedUpdate() 
	{
		if(GameClientInstance.CurrentRoom!=null)
		{
			if (Input.GetMouseButtonUp (0)) 
			{
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				
				if (Physics.Raycast (ray, out hit, 100f, mask)) 
				{
					switch(hit.transform.tag)
					{
						case "Player":
						{
							int id = hit.transform.gameObject.GetComponent<FSM_Character>().id;
							UIState.ClickOnPlayer (id);
							break;
							
						}
						case "Field":
						{
							if(!EventSystem.current.IsPointerOverGameObject())
							{
								UIState.ClickOnField (hit.point);
							}
							break;
						}
					}
				}	
			}
			if (Input.GetMouseButtonUp (1)) 
			{
				UIState.DeselectCharacter();
			}
		}
	}


	void OnApplicationQuit()
	{
		GameClientInstance.Disconnect();
	}
	public void NewGameButton()
	{
		UIMM.NewGameButt ();
	}
	public void QuitGameButton()
	{
		Application.Quit();
	}
	public void ClearButton()
	{
		this.GameClientInstance.ClearActions();
		foreach(FSM_Character c in board.Teams[(int)GameClientInstance.team].mates)
		{
			c.ClearActions();
		}
	}
	public void EndTurnButton()
	{
		UIState.DeselectCharacter ();
		UIState.EndTurnButton ();
	}
		
}