using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics;

[UpdateAfter(typeof(ResolveDamageSystem))]
public class DeathSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	//This job takes care to clean up character Entities, but leaves the connected GameObject so it can play the death animation
	[RequireComponentTag(typeof(IsDead), typeof(Health))]
    struct CharacterDeathJob : IJobForEachWithEntity<AnimationState>
    {
        public EntityCommandBuffer.Concurrent ECB;
        
        public void Execute(Entity entity,
							int entityIndex,
							ref AnimationState animationState)
        {
			animationState.TriggerIsDead = true;

			//Entity is destroyed, but not its corresponding GameObject (which is by now playing the death animation)
			ECB.DestroyEntity(entityIndex, entity);

			//Notice that AnimationState will survive, because it's a System State Component
        }
    }

	//This job will clean up "pure" object entities (no animation, no GameObject connected).
	//The IsDead component is not actually needed here, but requested here for the sake of shortening the code,
	//since there's no version of IJobForEachWithEntity without a component.
	[RequireComponentTag(typeof(Health))]
	[ExcludeComponent(typeof(AnimationState))]
	struct ObjectDeathJob : IJobForEachWithEntity<IsDead>
    {
    	public EntityCommandBuffer.Concurrent ECB;

		public void Execute(Entity entity,
							int entityIndex,
							[ReadOnly] ref IsDead isDead)
		{
			ECB.DestroyEntity(entityIndex, entity);
		}
	}
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		//Requesting a concurrent EntityCommandBuffer that both jobs will write commands into
		EntityCommandBuffer.Concurrent endSimECB = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();

        var job = new CharacterDeathJob()
		{
			ECB = endSimECB,
		};
		JobHandle handle = job.Schedule(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(handle);

		var job2 = new ObjectDeathJob()
		{
			ECB = endSimECB,
		};
		JobHandle handle2 = job2.Schedule(this, handle);
		EndSimECBSystem.AddJobHandleForProducer(handle2);

        return handle2;
    }
}