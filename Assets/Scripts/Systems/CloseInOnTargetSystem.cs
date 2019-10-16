using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class CloseInOnTargetSystem : JobComponentSystem
{	
	[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
    struct CloseInOnTargetJob : IJobForEach<MovementInput, Translation, Target, AttackRange>
    {
		[ReadOnly] public ComponentDataFromEntity<Translation> targetData;

        public void Execute(ref MovementInput movement,
							[ReadOnly] ref Translation translation,
							[ReadOnly] ref Target target,
							[ReadOnly] ref AttackRange attackRange)
        {
			if(!targetData.Exists(target.Entity) || !target.HasTarget)
			{
				movement.MoveAmount = float3.zero; //stops the Entity
			}
			else
			{
				Translation targetTranslation = targetData[target.Entity];
				if(math.lengthsq(targetTranslation.Value - translation.Value) > attackRange.Range * attackRange.Range)
				{
					movement.MoveAmount = math.normalize(targetTranslation.Value - translation.Value);
				}
				else
				{
					movement.MoveAmount = float3.zero; //stops the Entity
				}
			}
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		ComponentDataFromEntity<Translation> targetData = GetComponentDataFromEntity<Translation>(true);

        var job = new CloseInOnTargetJob();
		job.targetData = targetData;
        return job.Schedule(this, inputDependencies);
    }
}