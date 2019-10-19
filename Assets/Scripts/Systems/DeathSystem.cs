using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(ResolveDamageSystem))]
public class DeathSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	[RequireComponentTag(typeof(IsDead), typeof(Health))]
    struct DeathJob : IJobForEachWithEntity<AnimationState>
    {
        public EntityCommandBuffer.Concurrent ECB;
        
        public void Execute(Entity entity,
							int entityIndex,
							ref AnimationState animationState)
        {
			animationState.TriggerIsDead = true;

			//remove all the components that make the entity participate in input and gameplay
			ECB.RemoveComponent<CopyTransformToGameObject>(entityIndex, entity);
			ECB.RemoveComponent<MovementInput>(entityIndex, entity);
			ECB.RemoveComponent<AttackInput>(entityIndex, entity);
			ECB.RemoveComponent<Target>(entityIndex, entity);
			ECB.RemoveComponent<AttackRange>(entityIndex, entity);
			ECB.RemoveComponent<AlertRange>(entityIndex, entity);
			ECB.RemoveComponent<Health>(entityIndex, entity); //this avoids death from happening again
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new DeathJob();
		job.ECB = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();
		JobHandle handle = job.Schedule(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(handle);

        return handle;
    }
}