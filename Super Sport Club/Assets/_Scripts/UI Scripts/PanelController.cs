using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PanelController : MonoBehaviour 
{
	[SerializeField] Button buttFab;
	//List <Button> buttons;
	RectTransform rectTran;
	int buttCount = 0;
	void Awake()
	{
		rectTran = transform as RectTransform;
		//buttons = new List<Button>();
	}

	public Button AddButton(string name, bool StayInWorlPosition)
	{
		buttCount++;
		rectTran.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, 50 * buttCount);
		Button newButton = GameObject.Instantiate (buttFab) as Button;
		newButton.transform.SetParent (rectTran, false);
		newButton.GetComponentInChildren<Text> ().text = name;
		return newButton;

	}
}
