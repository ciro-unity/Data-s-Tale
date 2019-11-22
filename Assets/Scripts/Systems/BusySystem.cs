using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//This system takes care of resolving waits, by checking the time on Busy components and removing them when necessary.
public class BusySystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
    
	//This job checks the time on Busy components and removes them when they are "expired"
    struct ResolveWaitJob : IJobForEachWithEntity<Busy>
    {
		public float currentTime;
        public EntityCommandBuffer.Concurrent ECB;

        public void Execute(Entity entity, int entityIndex, [ReadOnly] ref Busy busy)
        {
			if(busy.Until <= currentTime)
			{
				ECB.RemoveComponent<Busy>(entityIndex, entity);
			}
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ResolveWaitJob()
		{
			currentTime = Time.time,
			ECB = EndSimECBSystem.CreateCommandBuffer().ToConcurrent()
		};

		JobHandle jobHandle = job.Schedule(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}