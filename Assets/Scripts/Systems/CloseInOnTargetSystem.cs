using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//This system takes care of characters that have a Target and need to move towards it to attack
//Basically it's a rudimentary AI, for now only used by Enemies
[UpdateBefore(typeof(AttackSystem))]
public class CloseInOnTargetSystem : JobComponentSystem
{	
	[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
	[ExcludeComponent(typeof(Busy))]
    struct CloseInOnTargetJob : IJobForEach<MovementInput, Translation, Target, AttackRange, AttackInput>
    {
		[ReadOnly] public ComponentDataFromEntity<Translation> targetData;

        public void Execute(ref MovementInput movement,
							[ReadOnly] ref Translation translation,
							[ReadOnly] ref Target target,
							[ReadOnly] ref AttackRange attackRange,
							ref AttackInput attackInput)
        {
			Translation targetTranslation = targetData[target.Entity];
			if(math.lengthsq(targetTranslation.Value - translation.Value) > attackRange.Range * attackRange.Range)
			{
				//Target is too far, move towards it
				movement.MoveAmount = math.normalize(targetTranslation.Value - translation.Value);
			}
			else
			{
				//Target is in range, attack
				movement.MoveAmount = float3.zero; //stops the Entity
				attackInput.Attack = true;
			}
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		//Job 1
		//Prepare an array of Translation components so Entities can access the position of their targets
		ComponentDataFromEntity<Translation> targetData = GetComponentDataFromEntity<Translation>(true);

        var job = new CloseInOnTargetJob();
		job.targetData = targetData;

        return job.Schedule(this, inputDependencies);
    }
}