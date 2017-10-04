public class Claw : Weapon
{

	public void Reset()
	{
		baseDamage = 10f;
		durability = 100f;
		durabilityLossPerHit = 0f;
		staminaCost = 10f;
		force = 20;
	}
}