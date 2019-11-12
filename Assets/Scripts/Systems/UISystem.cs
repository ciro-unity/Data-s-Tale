using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

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