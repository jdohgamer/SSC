using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterData: ScriptableObject
{
	public string Name;
	public int ID;
	public float Strength, Speed, Defense;
	
	public CharacterData()
	{
		Name = "BoB";
		ID = -1;
		Strength = 5; Speed = 5; Defense = 5;
	}
	
	public CharacterData(string name, int id, Vector3 stats)
	{
		Name = name;
		ID = id;
		Strength = stats.x; Speed = stats.y; Defense = stats.z;
	}
}
