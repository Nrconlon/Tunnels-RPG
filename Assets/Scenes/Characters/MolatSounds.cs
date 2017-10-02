using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolatSounds : MonoBehaviour {
	AudioSource mouthSource;
	AudioSource itemSource;

	[SerializeField] AudioClip chasing;
	[SerializeField] AudioClip powerSwing;
	[SerializeField] AudioClip[] shieldImpacts;
	[SerializeField] AudioClip shieldBreak;
	[SerializeField] AudioClip maceBreak;
	[SerializeField] AudioClip[] gotHit;
	[SerializeField] AudioClip[] died;
	[SerializeField] AudioClip pickUpItem;
	[SerializeField] AudioClip amazing;

	private void Start()
	{
		mouthSource = gameObject.AddComponent<AudioSource>();
		itemSource = gameObject.AddComponent<AudioSource>();
		mouthSource.loop = false;
		mouthSource.pitch = 0.9f + 0.2f * Random.value;

		itemSource.loop = false;
	}
	public void ChasingSoundEffect()
	{
		PlayClip(chasing, mouthSource);
	}
	public void PowerSwingSoundEffect()
	{
		PlayClip(powerSwing, mouthSource);
	}
	public void ShieldBreakSoundEffect()
	{
		PlayClip(shieldBreak, itemSource);
	}
	public void MaceBreakSoundEffect()
	{
		PlayClip(maceBreak, itemSource);
	}
	public void ShieldImpactsSoundEffect()
	{
		PlayClip(shieldImpacts, itemSource);
	}
	public void GotHitSoundEffect()
	{
		PlayClip(gotHit, mouthSource);
	}
	public void DiedSoundEffect()
	{
		PlayClip(died, mouthSource);
	}

	public void PickUpItemSoundEffect()
	{
		PlayClip(pickUpItem, itemSource);
	}

	public void AmazingSoundEffect()
	{
		PlayClip(amazing, mouthSource);
	}



	private void PlayClip(AudioClip clip, AudioSource audioSource)
	{
		audioSource.clip = clip;
		audioSource.Play();
	}
	private void PlayClip(AudioClip[] clips, AudioSource audioSource)
	{
		audioSource.clip = clips[Random.Range(0, clips.Length)];
		audioSource.Play();
	}
}
