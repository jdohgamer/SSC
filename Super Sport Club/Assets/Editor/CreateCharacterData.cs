using UnityEngine;
using UnityEditor;

public class CreateCharacterData
{
	[MenuItem("Assets/Create/CharacterData")]
	public static void CreateAsset ()
	{
		ScriptableObjectUtility.CreateAsset<CharacterData> ();
	}
}