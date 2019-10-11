using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class EnemyAISystem : JobComponentSystem
{
	private EntityQuery playerEntityQuery;

	protected override void OnCreate()
	{
		playerEntityQuery = GetEntityQuery(typeof(PlayerTag), typeof(Translation));
	}
		
    [BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
    struct EnemyAISystemJob : IJobForEach<Movement, Translation>
    {
       public float3 playerTranslation;
        
        public void Execute(ref Movement movement, [ReadOnly] ref Translation translation)
        {
            movement.MoveAmount = math.normalize(playerTranslation - translation.Value);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new EnemyAISystemJob();
		
		//Find the player entity, and pass its Translation component to the job
		NativeArray<Entity> playerArray = playerEntityQuery.ToEntityArray(Allocator.TempJob);
		Entity player = playerArray[0];
		job.playerTranslation = World.EntityManager.GetComponentData<Translation>(player).Value;
		playerArray.Dispose();

        return job.Schedule(this, inputDependencies);
    }
}