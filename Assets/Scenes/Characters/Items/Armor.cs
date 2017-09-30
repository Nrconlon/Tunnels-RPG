
public class Armor : Item
{
	public void BlockAHit(float force)
	{
		HandleDurabilityLoss(force);
	}

	public void Reset()
	{
		durability = 100f;
	}
	protected override void PlayBreakSound()
	{
		
	}

}
