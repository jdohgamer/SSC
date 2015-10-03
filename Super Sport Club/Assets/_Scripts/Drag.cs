using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public static bool bDragging;
	public CustomGameClient gameClient;
	public bool dragOnSurfaces = true;	
	public int index;
	//[SerializeField] GUIController GUI;
	public UISetPiece gui;
	public string PlayerPosition
	{
		get{return playerPosition;}
		set{playerPosition = value; text.text = playerPosition;}
	}
	[SerializeField] GameObject dragFab;
	private string playerPosition;
	private Text text;
	private GameObject m_DraggingIcon;
	private RectTransform m_DraggingPlane;
	LayerMask mask;// = 1<<LayerMask.NameToLayer("Ground");
	Ray ray;
	RaycastHit hit;
	Image image;
	
	void Awake()
	{
		text = GetComponentInChildren<Text>();
		text.text = PlayerPosition;
		image = GetComponent<Image>();
		mask = 1<<LayerMask.NameToLayer("Ground");
		gameClient = new CustomGameClient();
	}
	public void OnBeginDrag(PointerEventData eventData)
	{
		var canvas = FindInParents<Canvas>(gameObject);
		if (canvas == null)
			return;
		bDragging = true;
		StartCoroutine("GetHigh");
		image.enabled = false;
		text.enabled = false;
		// We have clicked something that can be dragged.
		// What we want to do is create an icon for this.
		m_DraggingIcon = Instantiate(dragFab,transform.position,Quaternion.identity)as GameObject;//new GameObject("icon");
		
		m_DraggingIcon.transform.SetParent (canvas.transform, false);
		m_DraggingIcon.transform.SetAsLastSibling();
		
		//var image = m_DraggingIcon.AddComponent<Image>();
		// The icon will be under the cursor.
		// We want it to be ignored by the event system.
		CanvasGroup group = m_DraggingIcon.AddComponent<CanvasGroup>();
		group.blocksRaycasts = false;
		
		//image.sprite = GetComponent<Image>().sprite;
		
		if (dragOnSurfaces)
			m_DraggingPlane = transform as RectTransform;
		else
			m_DraggingPlane = canvas.transform as RectTransform;
		
		SetDraggedPosition(eventData);
	}
	
	public void OnDrag(PointerEventData data)
	{
		if (m_DraggingIcon != null)
			SetDraggedPosition(data);
	}
	
	IEnumerator GetHigh()
	{
		while(Drag.bDragging)
		{
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			
			if (Physics.Raycast (ray, out hit, 100f, mask) && hit.transform.tag == "Field") 
			{
				Grid_Setup.Instance.HighlightSingle(hit.point);
			}
			yield return new WaitForSeconds(0.1f);
		}

	}
	private void SetDraggedPosition(PointerEventData data)
	{
		if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
			m_DraggingPlane = data.pointerEnter.transform as RectTransform;
		
		var rt = m_DraggingIcon.GetComponent<RectTransform>();
		Vector3 globalMousePos;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
		{
			rt.position = globalMousePos;
			rt.rotation = m_DraggingPlane.rotation;
		}
	}
	
	public void OnEndDrag(PointerEventData eventData)
	{
		if (m_DraggingIcon != null)
		{
			Destroy(m_DraggingIcon);
			bDragging = false;
			Grid_Setup.Instance.TurnOffSingle();
			StopCoroutine("GetHigh");
			if (Physics.Raycast (ray, out hit, 100f, mask) && hit.transform.tag == "Field") 
			{
				if (gui.PlaceCharacter (hit.point)) 
				{
					Grid_Setup.Instance.SetCharacter ((int)gameClient.team, index, hit.point);
					DisableMe ();
				} else {
					EnableMe ();
				}
			}
			//Destroy(gameObject);
		}
	}
	public void DisableMe()
	{
		this.gameObject.SetActive(false);
	}
	public void EnableMe()
	{
		this.gameObject.SetActive(true);
		image.enabled = true;
		text.enabled = true;
	}
	
	static public T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null) return null;
		var comp = go.GetComponent<T>();
		
		if (comp != null)
			return comp;
		
		Transform t = go.transform.parent;
		while (t != null && comp == null)
		{
			comp = t.gameObject.GetComponent<T>();
			t = t.parent;
		}
		return comp;
	}
}
