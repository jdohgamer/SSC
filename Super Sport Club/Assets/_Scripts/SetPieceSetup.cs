using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SetPieceSetup : MonoBehaviour//, IPointerClickHandler 
{
	Grid_Setup board;
	Image[] portraits;
	Image imageFab;
	FSM_Character[] characters;
	CharacterData[] data;
	RectTransform characterPanel;
	int teamSize = 5;
	
	void Setup()
	{
		data = new CharacterData[teamSize];
		portraits = new Image[teamSize];
		for(int i = 0; i<teamSize; i++)
		{
			
			
		}
	}
}
