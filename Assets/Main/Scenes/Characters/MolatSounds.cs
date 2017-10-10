using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolatSounds : MonoBehaviour {
	AudioSource mouthSource;
	AudioSource itemSource;
	AudioSource footSource;

	[SerializeField] AudioClip fleeing;
	[SerializeField] AudioClip powerSwing;
	[SerializeField] AudioClip[] shieldImpacts;
	[SerializeField] AudioClip shieldBreak;
	[SerializeField] AudioClip maceBreak;
	[SerializeField] AudioClip[] gotHit;
	[SerializeField] AudioClip[] died;
	[SerializeField] AudioClip pickUpItem;
	[SerializeField] AudioClip amazing;
	[SerializeField] AudioClip[] footSounds;

	float footStepsTimer;

	private void Start()
	{
		mouthSource = gameObject.AddComponent<AudioSource>();
		itemSource = gameObject.AddComponent<AudioSource>();
		footSource = gameObject.AddComponent<AudioSource>();

		mouthSource.loop = false;
		mouthSource.pitch = 0.9f + 0.2f * Random.value;

		itemSource.loop = false;

		footSource.loop = false;
		footSource.pitch = 0.9f + 0.2f * Random.value;
		footSource.volume = 0.2f;
	}
	public void FootStepSoundEffect()
	{
		if (!footSource.isPlaying && footStepsTimer < Time.time)
		{
			footStepsTimer = Time.time + Random.Range(0.3f,0.5f);
			PlayClip(footSounds, footSource);
		}
	}
	public void FleeingSoundEffect()
	{
		PlayClip(fleeing, mouthSource);
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
