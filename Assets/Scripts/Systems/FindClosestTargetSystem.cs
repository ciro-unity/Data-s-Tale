using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class FindClosestTargetSystem : JobComponentSystem
{
	public EntityQuery playersGroup;
	public EntityQuery enemyGroup;

	protected override void OnCreate()
	{
		playersGroup = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<Translation>(), ComponentType.Exclude<IsDead>());
		enemyGroup = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>(), ComponentType.ReadOnly<Translation>(), ComponentType.Exclude<IsDead>());
		
		RequireForUpdate(playersGroup);
		RequireForUpdate(enemyGroup);
	}


	[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
	struct FindClosestPlayerJob : IJobForEach<Target, Translation, AlertRange>
	{
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Entity> players;
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Translation> positions;

		public void Execute(ref Target target,
							[ReadOnly] ref Translation translation,
							[ReadOnly] ref AlertRange alertRange)
		{
			EvaluateList(ref target, translation, alertRange, players, positions);
		}
	}


	[BurstCompile]
	[RequireComponentTag(typeof(PlayerTag))]
	struct FindClosestEnemyJob : IJobForEach<Target, Translation, AlertRange>
	{
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Entity> enemies;
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Translation> positions;

		public void Execute(ref Target target,
							[ReadOnly] ref Translation translation,
							[ReadOnly] ref AlertRange alertRange)
		{
			EvaluateList(ref target, translation, alertRange, enemies, positions);
		}
	}


	public static void EvaluateList(ref Target target,
							Translation translation,
							AlertRange alertRange,
							NativeArray<Entity> entities,
							NativeArray<Translation> positions)
	{
		float closestSqDistance = alertRange.Range * alertRange.Range;
			target.HasTarget = false;
			for(int i=0; i<entities.Length; i++)
			{
				//check the squared distance to this Player and see if it's under the closest one we already found
				float currentSqDistance = math.lengthsq(positions[i].Value - translation.Value);
				if(currentSqDistance < closestSqDistance)
				{
					target.Entity = entities[i];
					closestSqDistance = currentSqDistance;
					target.HasTarget = true;
				}
			}
	}
    
	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		NativeArray<Entity> players = playersGroup.ToEntityArray(Allocator.TempJob, out JobHandle handle1);
		NativeArray<Translation> playerPositions = playersGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle handle2);
		inputDependencies = JobHandle.CombineDependencies(inputDependencies, handle1, handle2);

		// All enemies finding the closest player
		var job1 = new FindClosestPlayerJob()
		{
			players = players,
			positions = playerPositions,
		};

		JobHandle job1Handle = job1.Schedule(this, inputDependencies);

		NativeArray<Entity> enemies = enemyGroup.ToEntityArray(Allocator.TempJob, out JobHandle handle3);
		NativeArray<Translation> enemyPositions = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle handle4);
		job1Handle = JobHandle.CombineDependencies(job1Handle, handle3, handle4);

		// All players finding the closest enemy
		var job2 = new FindClosestEnemyJob()
		{
			enemies = enemies,
			positions = enemyPositions,
		};

		JobHandle job2Handle = job2.Schedule(this, job1Handle);

		return job2Handle;
	}
}