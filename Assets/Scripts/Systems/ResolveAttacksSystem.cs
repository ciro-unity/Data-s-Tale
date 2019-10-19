using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(AttackSystem))]
public class ResolveAttacksSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

    struct ResolveAttackJob : IJobForEachWithEntity<Target, DealBlow>
    {
        public EntityCommandBuffer ECB;
		public float currentTime;
        [NativeDisableParallelForRestriction] public BufferFromEntity<Damage> damages;

        public void Execute(Entity entity,
							int entityIndex,
							[ReadOnly] ref Target target,
							[ReadOnly] ref DealBlow dealBlow)
        {
			if(dealBlow.When <= currentTime
				&& target.HasTarget
				&& target.Entity != Entity.Null)
			{
				if(damages.Exists(target.Entity))
				{
					damages[target.Entity].Add(new Damage{ Amount = dealBlow.DamageAmount });
				}

				ECB.RemoveComponent<DealBlow>(entity);
			}
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ResolveAttackJob();
		job.currentTime = Time.time;
		job.ECB = EndSimECBSystem.CreateCommandBuffer();
		job.damages = GetBufferFromEntity<Damage>(false);

		//This job is writing to different buffers of Damage data of different entities.
		//Since we can't predict the order of write operations, this job is scheduled on a single thread.
        JobHandle handle = job.ScheduleSingle(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(handle);

		return handle;
    }
}