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


	[ExcludeComponent(typeof(Busy), typeof(Target))]
	struct EmptyAttackJob : IJobForEachWithEntity<AttackInput>
	{
		public float currentTime;
        public EntityCommandBuffer.Concurrent ECB;

		public void Execute(Entity entity, int index,
							[ReadOnly] ref AttackInput attackInput)
		{
			if(attackInput.Attack == true)
			{
				float busyUntil = currentTime + attackInput.AttackLength;
				ECB.AddComponent<Busy>(index, entity, new Busy{ Until = busyUntil });
			}
		}
	}

	[ExcludeComponent(typeof(Busy))]
    struct SuccessfulAttackJob : IJobForEachWithEntity<Target, AttackInput, Translation, Rotation>
    {
		public float currentTime;
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
				float busyUntil = currentTime + attackInput.AttackLength;
				ECB.AddComponent<Busy>(entityIndex, entity, new Busy{ Until = busyUntil });
				
				//snap rotation to look at the target
				float3 heading = math.normalize(translationData[target.Entity].Value - translation.Value);
				rotation.Value = quaternion.LookRotation(heading, math.up());

				//the target will receive damage
				ECB.AddComponent<DealBlow>(entityIndex, entity, new DealBlow{ When = busyUntil - .4f, DamageAmount = attackInput.AttackStrength });
				ECB.RemoveComponent<Target>(entityIndex, entity);
			}
        }
    }

	//This job just invalidates the attack input if the entity is busy
	//This is done to prevent the AnimationStateSystem job to pickup the attack bool and play an animation repeatedly
	struct CannotAttackJob : IJobForEach<Busy, AttackInput>
	{
		public void Execute([ReadOnly] ref Busy busy, ref AttackInput attackInput)
		{
			attackInput.Attack = false;
		}
	}
	
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		EntityCommandBuffer.Concurrent ecb = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();

		//Iterate on all entities which are busy
		var cantJob = new CannotAttackJob();
		JobHandle job1Handle = cantJob.Schedule(this, inputDependencies);

		//Now run a job on all entities that can attack and have a target
        var canJob = new SuccessfulAttackJob()
		{
			currentTime = Time.time,
			translationData = GetComponentDataFromEntity<Translation>(true),
			ECB = ecb,
		};

		JobHandle job2Handle = canJob.Schedule(this, job1Handle);
		EndSimECBSystem.AddJobHandleForProducer(job2Handle);

		//Run a job on the entities that can attack but have no valid target
		var emptyJob = new EmptyAttackJob()
		{
			currentTime = Time.time,
			ECB = ecb,
		};

		JobHandle job3Handle = emptyJob.Schedule(this, job2Handle);
		EndSimECBSystem.AddJobHandleForProducer(job3Handle);

        return job3Handle;
    }
}