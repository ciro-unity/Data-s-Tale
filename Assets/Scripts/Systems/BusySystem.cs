using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class BusySystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    //[BurstCompile]
    struct ResolveWaitJob : IJobForEachWithEntity<Busy>
    {
		public float currentTime;
        public EntityCommandBuffer.Concurrent m_EntityCommandBuffer;

        public void Execute(Entity entity, int entityIndex, [ReadOnly] ref Busy busy)
        {
			if(busy.Until <= currentTime)
			{
				m_EntityCommandBuffer.RemoveComponent<Busy>(entityIndex, entity); //TODO: is the entity index the right one to pass heßre??
			}
        }
    }

	protected override void OnCreate()
	{
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ResolveWaitJob();
		job.currentTime = Time.time;
		job.m_EntityCommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

		JobHandle jobHandle = job.Schedule(this, inputDependencies);
		m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}