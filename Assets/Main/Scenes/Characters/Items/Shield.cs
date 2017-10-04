using UnityEngine;


public class Shield : Item {
	public Vector3 lookAtDirection = Vector3.zero;
	public float maxAngleBlock = 45;

	public void BlockAHit(float force)
	{
		HandleDurabilityLoss(force);
		if(currentDurability > 0)
		{
			molatSounds.ShieldImpactsSoundEffect();
		}
	}

	private void Start()
	{
		currentDurability = durability;
	}

	public void Reset()
	{
		durability = 100f;
	}

	public void SetLookAtDirection(Vector3 lookAtDirection)
	{
		this.lookAtDirection = lookAtDirection;
	}

	protected override void PlayBreakSound()
	{
		molatSounds.ShieldBreakSoundEffect();
	}

	public override void StartUsing(Molat myMolat)
	{
		myMolat.LookAtDirectionDel += SetLookAtDirection;
	}

	public override void StopUsing(Molat myMolat)
	{
		myMolat.LookAtDirectionDel -= SetLookAtDirection;
	}
}
