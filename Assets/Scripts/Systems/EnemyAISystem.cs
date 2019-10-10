using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class EnemyAISystem : JobComponentSystem
{
    [BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
    struct EnemyAISystemJob : IJobForEach<Movement, Translation>
    {
       public Translation playerTranslation;
        
        public void Execute(ref Movement movement, [ReadOnly] ref Translation translation)
        {
            movement.MoveAmount = math.normalize(playerTranslation.Value - translation.Value);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new EnemyAISystemJob();
		job.playerTranslation = new Translation();
        return job.Schedule(this, inputDependencies);
    }
}