using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(ResolveAttacksSystem))]
public class ResolveDamageSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	[RequireComponentTag(typeof(Damage))]
    struct ResolveDamageJob : IJobForEachWithEntity<Health>
    {
        public EntityCommandBuffer.Concurrent ECB;
		[NativeDisableParallelForRestriction] public BufferFromEntity<Damage> damages;

        public void Execute(Entity entity,
							int entityIndex,
							ref Health health)
        {
			//Cumulate all damages, one by one
			for(int i=0; i<damages[entity].Length; i++)
			{
				health.Current -= damages[entity][i].Amount;
			}

			damages[entity].Clear();

			if(health.Current <= 0)
			{
				ECB.AddComponent<IsDead>(entityIndex, entity, new IsDead());
			}
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ResolveDamageJob();
		job.ECB = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();
		job.damages = GetBufferFromEntity<Damage>(false);

		JobHandle handle = job.Schedule(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(handle);

        return handle;
    }
}