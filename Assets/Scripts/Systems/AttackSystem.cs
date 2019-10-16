using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class AttackSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    //[BurstCompile]
	[ExcludeComponent(typeof(Busy))]
	[RequireComponentTag(typeof(Target))] //require it?
    struct CanAttackJob : IJobForEachWithEntity<AttackInput>
    {
		public float currentTime;
        public EntityCommandBuffer.Concurrent m_EntityCommandBuffer;
        
        public void Execute(Entity entity, int entityIndex, [ReadOnly] ref AttackInput attackInput)
        {
			if(attackInput.Attack == true)
			{
				float busyUntil = currentTime + 0.88f - .1f; //TODO: not hardcode these values
				m_EntityCommandBuffer.AddComponent<Busy>(entityIndex, entity, new Busy{Until = busyUntil});
			}
        }
    }

	//This job just invalidates the attack input if the entity is busy
	struct CannotAttackJob : IJobForEach<Busy, AttackInput>
	{
		public void Execute([ReadOnly] ref Busy busy, ref AttackInput attackInput)
		{
			attackInput = new AttackInput
			{
				Attack = false,
			};
		}
	}

	protected override void OnCreate()
	{
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}
	
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		//Iterate on all entities which are busy
		var cantJob = new CannotAttackJob();
		JobHandle cantJobHandle = cantJob.Schedule(this, inputDependencies);

		//Now run a job on all entities that can attack
        var canJob = new CanAttackJob();
		canJob.currentTime = Time.time;
		canJob.m_EntityCommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

		JobHandle canJobHandle = canJob.Schedule(this, cantJobHandle);
		m_EntityCommandBufferSystem.AddJobHandleForProducer(canJobHandle);

        return canJobHandle;
    }
}