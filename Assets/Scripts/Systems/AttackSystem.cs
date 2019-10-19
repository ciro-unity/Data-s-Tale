using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

public class AttackSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

    //[BurstCompile]
	[ExcludeComponent(typeof(Busy))]
    struct CanAttackJob : IJobForEachWithEntity<Target, AttackInput, Translation, Rotation>
    {
		public float currentTime;
		[ReadOnly] public ComponentDataFromEntity<IsDead> deadData;
		[ReadOnly] public ComponentDataFromEntity<Translation> translationData;
        public EntityCommandBuffer.Concurrent ECB;
        
        public void Execute(Entity entity,
							int entityIndex,
							ref Target target,
							[ReadOnly] ref AttackInput attackInput,
							[ReadOnly] ref Translation translation,
							ref Rotation rotation)
        {
			if(attackInput.Attack == true)
			{
				float busyUntil = currentTime + attackInput.AttackLength - .1f;
				ECB.AddComponent<Busy>(entityIndex, entity, new Busy{ Until = busyUntil });
				
				if(deadData.Exists(target.Entity))
				{
					//the target is dead
					target.HasTarget = false;
				}
				else
				{
					if(!target.HasTarget || target.Entity == Entity.Null)
					{
						return;
					}

					//snap rotation to look at the target
					float3 heading = math.normalize(translationData[target.Entity].Value - translation.Value);
					rotation.Value = quaternion.LookRotation(heading, math.up());

					//the target will receive damage
					ECB.AddComponent<DealBlow>(entityIndex, entity, new DealBlow{ When = busyUntil - .4f, DamageAmount = attackInput.AttackStrength });
				}
			}
        }
    }

	//This job just invalidates the attack input if the entity is busy
	struct CannotAttackJob : IJobForEach<Busy, AttackInput>
	{
		public void Execute([ReadOnly] ref Busy busy, ref AttackInput attackInput)
		{
			attackInput.Attack = false;
		}
	}
	
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		//Iterate on all entities which are busy
		var cantJob = new CannotAttackJob();
		JobHandle cantJobHandle = cantJob.Schedule(this, inputDependencies);

		//Now run a job on all entities that can attack
		ComponentDataFromEntity<IsDead> deadData = GetComponentDataFromEntity<IsDead>(true);
		ComponentDataFromEntity<Translation> translationData = GetComponentDataFromEntity<Translation>(true);

        var canJob = new CanAttackJob();
		canJob.currentTime = Time.time;
		canJob.deadData = deadData;
		canJob.translationData = translationData;
		canJob.ECB = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();

		JobHandle canJobHandle = canJob.Schedule(this, cantJobHandle);
		EndSimECBSystem.AddJobHandleForProducer(canJobHandle);

        return canJobHandle;
    }
}