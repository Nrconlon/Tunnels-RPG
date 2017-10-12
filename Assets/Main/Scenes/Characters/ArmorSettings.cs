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
		armorMaterial.material.SetTexture(Shader.PropertyToID("_diffusemap"), colors[index]);
	}
}
