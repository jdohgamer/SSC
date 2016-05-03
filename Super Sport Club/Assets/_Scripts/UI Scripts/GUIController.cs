using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class GUIController: MonoBehaviour
{

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
	[SerializeField] RectTransform MainMenu, Turn_Button, Submit_Button, Switch_Button, InGameHUD = null;
	[SerializeField] LayerMask mask;
	[SerializeField] Text infoText;
	private IUIState uiState;
	private MainGame mainGame;
	private Grid_Setup board;
	private bool bUpdatingInfo;

	void Awake()
	{
		mainGame = MainGame.Instance; //this starts the MainGame script
		UIMM = new UIMainMenu(this);
		UISP = new UISetPiece (this);
		UIHUD = new UIGameHUD(this, mainGame);
		UISOG = new UIShotState(this);
		UIState = UIMM;
	}

	void Update()
	{

		UIState.Update ();

		//if(CustomGameClient.ClientInstance.CurrentRoom!=null)
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
							UnitController u =  hit.transform.gameObject.GetComponent<UnitController> ();
							if(u.team == mainGame.CurrentTeamNum)
							{
								int id =u.id;
								UIState.ClickOnPlayer (id);
							}
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
		while (true) //make this event based
		{
			string side =  mainGame.CurrentTeamNum>0 ? "Right":"Left" ;
			infoText.text = string.Format(" Turn: {2}\n team: {0}. \n You're on the: {1} side. \n", mainGame.CurrentTeamNum , side, mainGame.TurnNumber);
			infoText.text += string.Format (" You have {0} moves left \n", mainGame.ActionsLeft);
			infoText.text += string.Format(" Opponenent ready: {0}\n", CustomGameClient.ClientInstance.HasOppSubmitted());
			infoText.text += string.Format(" Score: {0} : {1}\n", mainGame.TeamScore(0), mainGame.TeamScore(1));
			yield return new WaitForSeconds (1f);
		}
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
		Turn_Button.gameObject.SetActive(!set);
		Submit_Button.gameObject.SetActive(set);
	}
	void OnApplicationQuit()
	{
		CustomGameClient.ClientInstance.Disconnect();
	}
	public void NewOnlineGameButton()
	{
		mainGame.Connect();
		//UIState.ToSetPiece();
	}

	public void NewDevGameButton()
	{
		mainGame.NewGame();
		Switch_Button.gameObject.SetActive(true);
	}

	public void QuitGameButton()
	{
		Application.Quit();
	}
	public void ClearButton()
	{
		mainGame.ClearActions();
	}
	public void SwitchTeams()
	{
		UIState.DeselectCharacter ();
		mainGame.CurrentTeamNum = (mainGame.CurrentTeamNum+1)%2;
	}
	public void EndTurnButton()
	{
		UIState.DeselectCharacter ();
		mainGame.EndTurn();
	}
	public void SubmitTeam()
	{	
		mainGame.SubmitTeam();
	}
}