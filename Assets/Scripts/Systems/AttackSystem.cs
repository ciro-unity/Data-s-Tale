using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

//This system takes care of AttackInput, and produces a different result whether the Entity has a Target or not
public class AttackSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	//This job affects Entities that attack with no Target. This should only be possible for the player character
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

	//This job affects entities that have a target, so they hit it
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
				//The Busy component excludes the character from participating in many systems until the attack animaiton is over
				float busyUntil = currentTime + attackInput.AttackLength;
				ECB.AddComponent<Busy>(entityIndex, entity, new Busy{ Until = busyUntil });
				
				//Snap rotation to look at the target
				float3 heading = math.normalize(translationData[target.Entity].Value - translation.Value);
				heading.y = 0f;
				rotation.Value = quaternion.LookRotation(heading, math.up());

				//The target will receive damage at the time set in the DealBlow component
				ECB.AddComponent<DealBlow>(entityIndex, entity, new DealBlow{ When = busyUntil - .4f, DamageAmount = attackInput.AttackStrength });
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
		//Obtain an EntityCommandBuffer which all jobs will write commands into
		EntityCommandBuffer.Concurrent EndSimECB = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();

		//Job 1
		//Iterate on all entities which are busy
		var cantJob = new CannotAttackJob();
		JobHandle job1Handle = cantJob.Schedule(this, inputDependencies);

		//Job 2
		//Now run a job on all entities that can attack and have a target
        var canJob = new SuccessfulAttackJob()
		{
			currentTime = Time.time,
			translationData = GetComponentDataFromEntity<Translation>(true),
			ECB = EndSimECB,
		};

		JobHandle job2Handle = canJob.Schedule(this, job1Handle);
		EndSimECBSystem.AddJobHandleForProducer(job2Handle);

		//Job 3
		//Run a job on the entities that can attack but have no valid target
		var emptyJob = new EmptyAttackJob()
		{
			currentTime = Time.time,
			ECB = EndSimECB,
		};

		JobHandle job3Handle = emptyJob.Schedule(this, job2Handle);
		EndSimECBSystem.AddJobHandleForProducer(job3Handle);

        return job3Handle;
    }
}