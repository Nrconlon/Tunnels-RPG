public class Claw : Weapon
{

	public void Reset()
	{
		baseDamage = 10f;
		durability = 100f;
		durabilityLossPerHit = 0f;
		force = 20;
	}
}