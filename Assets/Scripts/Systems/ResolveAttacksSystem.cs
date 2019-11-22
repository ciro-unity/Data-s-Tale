using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//This system resolves attacks, dealing the intended damage to the Target entity.
//Damage entries are added to a DynamicBuffer, so it's possible that an entity receives multiple damage
//from different attackers in the same frame.
[UpdateAfter(typeof(AttackSystem))]
public class ResolveAttacksSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	//This job finds all of the attackers that have a DealBlow component pending, and applies the damage to the target
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
			if(dealBlow.When <= currentTime)
			{
				if(damages.Exists(target.Entity))
				{
					damages[target.Entity].Add(new Damage{ Amount = dealBlow.DamageAmount }); //Add element to the DynamicBuffer
				}

				ECB.RemoveComponent<DealBlow>(entity);
			}
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		//Job 1
        var job = new ResolveAttackJob()
		{
			currentTime = Time.time,
			ECB = EndSimECBSystem.CreateCommandBuffer(),
			damages = GetBufferFromEntity<Damage>(false),
		};

		//This job is writing to different buffers of Damage data of different entities.
		//Since we can't predict the order of write operations, this job is scheduled on a single thread.
        JobHandle handle = job.ScheduleSingle(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(handle);

		return handle;
    }
}