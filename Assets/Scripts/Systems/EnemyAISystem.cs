using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class EnemyAISystem : JobComponentSystem
{
	protected override void OnCreate()
	{
		//This would make it so that if there is no entity with a PlayerTag, this system is not even run
		RequireSingletonForUpdate<PlayerTag>();
	}
	
	[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
    struct EnemyAISystemJob : IJobForEach<MovementInput, Translation>
    {
       public float3 playerTranslation;	
        
        public void Execute(ref MovementInput movement, [ReadOnly] ref Translation translation)
        {
            movement.MoveAmount = math.normalize(playerTranslation - translation.Value);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new EnemyAISystemJob();
		
		//Find the player entity, and pass its Translation component to the job
		//Here we're assuming that there's only one entity with PlayerTag
		job.playerTranslation = EntityManager.GetComponentData<Translation>(GetSingletonEntity<PlayerTag>()).Value;

        return job.Schedule(this, inputDependencies);
    }
}