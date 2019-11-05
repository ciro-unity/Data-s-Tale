using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class BusySystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

    //[BurstCompile]
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

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ResolveWaitJob();
		job.currentTime = Time.time;
		job.ECB = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();

		JobHandle jobHandle = job.Schedule(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}