using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class WanderSystem : JobComponentSystem
{
	BeginSimulationEntityCommandBufferSystem BeginSimECBSystem;

	protected override void OnCreate()
	{
        BeginSimECBSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
	}

	[ExcludeComponent(typeof(Busy))]
    struct WanderSystemJob : IJobForEachWithEntity<MovementInput, Wanderer>
    {
		public float currentTime;
        public EntityCommandBuffer.Concurrent ECB;

        public void Execute(Entity entity, int entityIndex, ref MovementInput movementInput, [ReadOnly] ref Wanderer wanderer)
        {
			float3 randomDirection = new float3(wanderer.RandomSeed.NextFloat(-1f, 1f), 0f, wanderer.RandomSeed.NextFloat(-1f, 1f));
			movementInput.MoveAmount = randomDirection;

			//add a Busy component so it waits a while before calculating a new random direction
			Busy busy = new Busy{ Until = currentTime + 2f }; //TODO: not hardcode the wait
			ECB.AddComponent<Busy>(entityIndex, entity, busy);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new WanderSystemJob();
		job.currentTime = Time.time;
		job.ECB = BeginSimECBSystem.CreateCommandBuffer().ToConcurrent();
		
        JobHandle handle = job.Schedule(this, inputDependencies);
		BeginSimECBSystem.AddJobHandleForProducer(handle);

		return handle;
    }
}