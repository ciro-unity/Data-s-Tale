using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//This system takes care of attracting Collectables to the player if within a certain range and,
//if close enough, destroys them and adds their value to the score.
public class CollectableSystem : JobComponentSystem
{
	//Config variables
	private float attractionRange = 3f;
	private float collectableSpeed = 10f;
	
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	[ExcludeComponent(typeof(Busy))]
    struct AttractCollectableJob : IJobForEachWithEntity<Translation, Collectable>
    {
		//Incoming data for the Player entity
        public Translation playerTranslation;
		
		//We can use the NativeDisableParallelForRestriction attribute because we are never writing to the same position,
		//since we are just using Add to add new elements to the List. Thus, we can allow the write operation safely in jobs.
		[NativeDisableParallelForRestriction] public NativeList<int> playerScores;

		public EntityCommandBuffer.Concurrent ECB;
		public float attrRange;
		public float collSpd;
		public float deltaTime;

        public void Execute(Entity entity, int jobIndex, ref Translation translation, [ReadOnly] ref Collectable collectable)
        {
			float3 dir = playerTranslation.Value - translation.Value + new float3(0f, 1f, 0f);
			float sqrDistance = math.lengthsq(dir);

			if(sqrDistance <= attrRange * attrRange)
			{
				//Move the collectable towards the player
				float distanceBasedAttenuation = math.max(sqrDistance, 1f);
				translation.Value += math.normalizesafe(dir) * deltaTime * collSpd / distanceBasedAttenuation;

				if(sqrDistance < .2f)
				{
					playerScores.Add(collectable.Value);
					ECB.DestroyEntity(jobIndex, entity); //Destroy the collectable
				}
			}
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		//Requesting a concurrent EntityCommandBuffer that the job will write commands into
		EntityCommandBuffer.Concurrent endSimECB = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();

		//Fetch data about the Player character to be used for distance calculations and more
		//Note: this system assumes there's only one Player. If more than one exists, you will see errors pop up
		Entity playerEntity = GetSingletonEntity<PlayerTag>();
		Translation playerTranslationComp = EntityManager.GetComponentData<Translation>(playerEntity);

		//The sub-jobs will store all of the scores coming from collectables in one list
		NativeList<int> playerScoreList = new NativeList<int>(Allocator.TempJob);

        var job = new AttractCollectableJob()
		{
			playerTranslation = playerTranslationComp,
			attrRange = attractionRange,
			collSpd = collectableSpeed,
			deltaTime = Time.deltaTime,
			playerScores = playerScoreList,
			ECB = endSimECB,
		};

		JobHandle handle1 = job.Schedule(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(handle1);

		//We ensure that the job is complete, before we read back the value of the all the scores in the list.
		handle1.Complete();

		if(playerScoreList.Length != 0)
		{
			int totalScore = EntityManager.GetComponentData<Score>(playerEntity).Value;
			
			//Iterate the NativeList and gather all the scores coming from the collectables picked up on this frame.
			for(int i=0; i<playerScoreList.Length; i++)
			{
				totalScore += playerScoreList[i];
			}
			EntityManager.SetComponentData<Score>(playerEntity, new Score{ Value = totalScore});
			EntityManager.AddComponent<UpdateScoreUI>(playerEntity); //will trigger the query in the UISystem that updates the score UI
		}
		playerScoreList.Dispose();

        return handle1;
    }
}