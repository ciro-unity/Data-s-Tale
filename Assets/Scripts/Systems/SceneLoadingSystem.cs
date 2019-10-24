using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
public class SceneLoadingSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EndSimECBSystem;

	protected override void OnCreate()
	{
        EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		//This will ensure that if the player is not present, the system is not run
		RequireSingletonForUpdate<PlayerTag>();
	}

	[ExcludeComponent(typeof(RequestSceneLoaded))]
	struct LoadSceneJob : IJobForEachWithEntity<SceneBoundingVolume>
	{
		public EntityCommandBuffer.Concurrent ECB;
		public Translation playerPosition;

		public void Execute(Entity entity, int index,
							[ReadOnly] ref SceneBoundingVolume boundingVolume)
		{
			AABB sceneAABB = (AABB)boundingVolume.Value;
			sceneAABB.Center += new float3(0f, 0f, -5f);
			sceneAABB.Extents += new float3(18f, 1f, 30f);

			if(sceneAABB.Contains(playerPosition.Value))
			{
				ECB.AddComponent<RequestSceneLoaded>(index, entity, new RequestSceneLoaded());
			}
		}
	}

	[RequireComponentTag(typeof(RequestSceneLoaded))]
	struct UnloadSceneJob : IJobForEachWithEntity<SceneBoundingVolume>
	{
		public EntityCommandBuffer.Concurrent ECB;
		public Translation playerPosition;

		public void Execute(Entity entity, int index,
							[ReadOnly] ref SceneBoundingVolume boundingVolume)
		{
			AABB sceneAABB = (AABB)boundingVolume.Value;
			sceneAABB.Center += new float3(0f, 0f, -5f);
			sceneAABB.Extents += new float3(18f, 1f, 30f);

			if(!sceneAABB.Contains(playerPosition.Value))
			{
				ECB.RemoveComponent<RequestSceneLoaded>(index, entity);
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		EntityCommandBuffer.Concurrent ecb = EndSimECBSystem.CreateCommandBuffer().ToConcurrent();
		Translation playerTranslation = EntityManager.GetComponentData<Translation>(GetSingletonEntity<PlayerTag>());

        var job = new LoadSceneJob();
		job.ECB = ecb;
		job.playerPosition = playerTranslation;

		var job2 = new UnloadSceneJob();
		job2.ECB = ecb;
		job2.playerPosition = playerTranslation;
        
		JobHandle jobHandle = job.Schedule(this, inputDependencies);
		EndSimECBSystem.AddJobHandleForProducer(jobHandle);

		JobHandle job2Handle = job2.Schedule(this, jobHandle);
		EndSimECBSystem.AddJobHandleForProducer(job2Handle);

		return job2Handle;
    }
}