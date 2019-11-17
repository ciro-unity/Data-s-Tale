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
	
			float duration = 0f;
			if(chanceToMove < .4f)
			{
				float3 direction;
				if(math.lengthsq(wanderer.InitialPosition-translation.Value) < maxDist*maxDist)
				{
					//move randomly
					float2 normalisedDir = wanderer.RandomSeed.NextFloat2Direction();
					direction = new float3(normalisedDir.x, 0f, normalisedDir.y);
					duration = 1f;
				}
				else
				{
					//move towards the initial position
					direction = math.normalizesafe(wanderer.InitialPosition-translation.Value);
					duration = 2f;
				}
				movementInput.MoveAmount = direction;
			}
			else
			{
				//stand still
				movementInput.MoveAmount = float3.zero;
				duration = 3f;
			}

			//add a Busy component so it waits a while before calculating a new random direction
			Busy busy = new Busy{ Until = currentTime + duration }; //TODO: not hardcode the wait
			ECB.AddComponent<Busy>(entityIndex, entity, busy);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		//Job 1
        var job = new WanderSystemJob()
		{
			currentTime = Time.time,
			ECB = BeginSimECBSystem.CreateCommandBuffer().ToConcurrent(),
			maxDist = 3f,
		};
		
        JobHandle handle = job.Schedule(this, inputDependencies);
		BeginSimECBSystem.AddJobHandleForProducer(handle);

		return handle;
    }
}