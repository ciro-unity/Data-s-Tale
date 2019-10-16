using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class WanderSystem : JobComponentSystem
{
	BeginSimulationEntityCommandBufferSystem BeginSimECBSystem;

	protected override void OnCreate()
	{
        BeginSimECBSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
	}

	[ExcludeComponent(typeof(Busy))]
    struct WanderSystemJob : IJobForEachWithEntity<MovementInput, Wanderer, Translation>
    {
		public float currentTime;
        public EntityCommandBuffer.Concurrent ECB;
		public float maxDist;

        public void Execute(Entity entity,
							int entityIndex,
							ref MovementInput movementInput,
							[ReadOnly] ref Wanderer wanderer,
							[ReadOnly] ref Translation translation)
        {
			float chanceToMove = wanderer.RandomSeed.NextFloat();
	
			if(chanceToMove < .4f)
			{
				float3 direction;
				if(math.lengthsq(wanderer.InitialPosition-translation.Value) < maxDist*maxDist)
				{
					//move randomly
					float2 normalisedDir = wanderer.RandomSeed.NextFloat2Direction();
					direction = new float3(normalisedDir.x, 0f, normalisedDir.y);
				}
				else
				{
					//move towards the initial position
					direction = math.normalizesafe(wanderer.InitialPosition-translation.Value);
				}
				movementInput.MoveAmount = direction;
			}
			else
			{
				//stand still
				movementInput.MoveAmount = float3.zero;
			}

			//add a Busy component so it waits a while before calculating a new random direction
			Busy busy = new Busy{ Until = currentTime + 3f }; //TODO: not hardcode the wait
			ECB.AddComponent<Busy>(entityIndex, entity, busy);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new WanderSystemJob();
		job.currentTime = Time.time;
		job.ECB = BeginSimECBSystem.CreateCommandBuffer().ToConcurrent();
		job.maxDist = 3f;
		
        JobHandle handle = job.Schedule(this, inputDependencies);
		BeginSimECBSystem.AddJobHandleForProducer(handle);

		return handle;
    }
}