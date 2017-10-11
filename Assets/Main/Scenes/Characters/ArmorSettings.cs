using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorSettings : MonoBehaviour {
	Renderer armorMaterial;
	[SerializeField] List<Texture> colors = new List<Texture>();
	// Use this for initialization
	private void Awake()
	{
		armorMaterial = GetComponent<Renderer>();
	}
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetColor(int index)
	{
		int realIndex = index % colors.Count;
		armorMaterial.material.SetTexture(1, colors[realIndex]);
	}
}
