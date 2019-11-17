using Unity.Entities;

//This system is in charge of making so that the ECS component Health belonging to the player
//is reflected in the UI, by calling a function on the singleton class UIManager
[UpdateBefore(typeof(ResolveDamageSystem))]
public class UISystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.WithAllReadOnly<PlayerTag>().ForEach((DynamicBuffer<Damage> damages, ref Health health) =>
		{
			if(damages.Length == 0)
			{
				return;
			}

			float newHealth = health.Current;
			for(int i=0; i<damages.Length; i++)
			{
				newHealth -= damages[i].Amount;
			}

			float percentage = newHealth/health.FullHealth;
			UIManager.Instance.SetHealthPerc(percentage);
		});
	}
}