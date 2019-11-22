using Unity.Entities;

//This system is in charge of making so that the ECS component Health and Score belonging to the player
//is reflected in the UI, by calling a function on the singleton class UIManager
[UpdateBefore(typeof(ResolveDamageSystem))]
public class UISystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		//This query finds the player every frame, because the Damage buffer is always present (although it might be empty)
		//TODO: optimise it somehow? is it worth it?
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

			float healthPercentage = newHealth/health.FullHealth;

			UIManager.Instance.SetHealthUIValue(healthPercentage);
		});

		//This query will find the player only when it has the UpdateScoreUI component, meaning the score has changed
		Entities.WithAllReadOnly<PlayerTag, UpdateScoreUI>().ForEach((Entity e, ref Score score) =>
		{
			UIManager.Instance.SetScoreUIValue(score.Value);
			PostUpdateCommands.RemoveComponent(e, typeof(UpdateScoreUI));
		});
	}
}