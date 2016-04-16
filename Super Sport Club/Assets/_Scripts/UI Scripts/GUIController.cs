using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

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
	public UIShotState UISOG;
	public Image panelFab, characterCardFab;
	public SpriteRenderer meterFab;
	public Button buttFab;
	public Canvas UIcan;
	public RectTransform CharacterPanel;
	[SerializeField] RectTransform MainMenu, InGameHUD = null;
	[SerializeField] string AppId;// set in inspector. this is called when the client loaded and is ready to start
	[SerializeField] float serviceInterval = 1;
	[SerializeField] LayerMask mask;
	[SerializeField] Text infoText;
	private IUIState uiState;
	//private CustomGameClient CustomGameClient.ClientInstance;
	private MainGame mainGame;
	private Grid_Setup board;
	private bool bUpdatingInfo;

	void Awake()
	{
		//this.board = GetComponent<Grid_Setup>();
		//Grid_Setup.Instance = this.board;
		//this.GameClientInstance  = new CustomGameClient();
		//this.GameClientInstance.AppId = AppId;  // edit this!
		//this.GameClientInstance.board = board;
		//this.GameClientInstance.gui = this;
		mainGame = MainGame.Instance;
		Application.runInBackground = true;
		CustomTypes.Register();
		UIMM = new UIMainMenu(this, mainGame);
		UISP = new UISetPiece (this, mainGame);
		UIHUD = new UIGameHUD(this, mainGame);
		UISOG = new UIShotState(this, mainGame);
		UIState = UIMM;
	}
	void OnEnable()
	{
		UnityEventManager.StartListeningInt("ScoreGoal", GoalScored);
	}
	void OnDisable()
	{
		UnityEventManager.StopListeningInt("ScoreGoal", GoalScored);
	}
	void GoalScored(int p)
	{
		//board.ResetBoard ();
		CustomGameClient.ClientInstance.ScorePoint (p);
		UIState = UISP;
	}

	void Update()
	{
//		timeSinceService += Time.deltaTime;
//		if (timeSinceService > serviceInterval)
//		{
//			this.GameClientInstance.Service();
//			timeSinceService = 0;
//		}
		UIState.Update ();

		if(CustomGameClient.ClientInstance.CurrentRoom!=null)
		{
			if (!bUpdatingInfo) 
			{
				StartCoroutine ("UpdateInfo");
			}
			if (Input.GetMouseButtonDown (0)) 
			{
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

				if (!EventSystem.current.IsPointerOverGameObject () && Physics.Raycast (ray, out hit, 100f, mask)) 
				{
					switch (hit.transform.tag) 
					{
					case "Player":
						{
							int id = hit.transform.gameObject.GetComponent<UnitController> ().id;
							UIState.ClickOnPlayer (id);
							break;

						}
					case "Field":
						{
							UIState.ClickOnField (hit.point);
							break;
						}
					}
				}	
			}
			if (Input.GetMouseButtonDown (1)) 
			{
				UIState.DeselectCharacter();
			}
		}
	}

	public IEnumerator UpdateInfo ()
	{
		bUpdatingInfo = true;
		while (CustomGameClient.ClientInstance.CurrentRoom!=null) 
		{
			string side =  (int)CustomGameClient.ClientInstance.team>0 ? "Right":"Left" ;
			infoText.text = string.Format(" Turn: {2}\n team: {0}. \n You're on the: {1} side. \n", (int)CustomGameClient.ClientInstance.team , side, CustomGameClient.ClientInstance.TurnNumber);
			infoText.text += string.Format (" You have {0} moves left \n", CustomGameClient.ClientInstance.ActionsLeft);
			infoText.text += string.Format(" Opponenent ready: {0}\n", CustomGameClient.ClientInstance.HasOppSubmitted());
			infoText.text += string.Format(" Score: {0} : {1}\n", CustomGameClient.ClientInstance.TeamScore(0), CustomGameClient.ClientInstance.TeamScore(1));
			yield return new WaitForSeconds (1f);
		}
		yield return null;
	}

	public void EnableHUD(bool set)
	{
		InGameHUD.gameObject.SetActive (set);
	}
	public void EnableMainMenu(bool set)
	{
		MainMenu.gameObject.SetActive (set);
	}
	public void EnableCharacterSelection(bool set)
	{
		CharacterPanel.gameObject.SetActive (set);
	}
	void OnApplicationQuit()
	{
		CustomGameClient.ClientInstance.Disconnect();
	}
	public void NewGameButton()
	{
		mainGame.Connect();
		mainGame.NewOnlineGame();
		UIState.ToSetPiece();
	}

	public void NewDevGameButton()
	{
		mainGame.NewLocalGame();
		UIState.ToSetPiece();
		//board.Generate();
	}

	public void QuitGameButton()
	{
		Application.Quit();
	}
	public void ClearButton()
	{
		mainGame.ClearActions();
//		this.GameClientInstance.ClearActions();
//		foreach(UnitController c in board.Teams[(int)GameClientInstance.team].mates)
//		{
//			c.ClearActions();
//		}
	}
	public void EndTurnButton()
	{
		UIState.DeselectCharacter ();
		mainGame.EndTurn();
	}
	public void SubmitTeam()
	{	
	}
}