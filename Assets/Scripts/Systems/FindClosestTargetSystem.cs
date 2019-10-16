using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class FindClosestTargetSystem : JobComponentSystem
{
	public EntityQuery playersGroup;

	protected override void OnCreate()
	{
		playersGroup = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<Translation>());
		
		RequireForUpdate(playersGroup);
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
			float closestSqDistance = alertRange.Range * alertRange.Range;
			target.HasTarget = false;
			for(int i=0; i<players.Length; i++)
			{
				//check the squared distance to this Player and see if it's under the closest one we already found
				float currentSqDistance = math.lengthsq(positions[i].Value - translation.Value);
				if(currentSqDistance < closestSqDistance)
				{
					target.Entity = players[i];
					closestSqDistance = currentSqDistance;
					target.HasTarget = true;
				}
			}
		}
	}
    
	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		NativeArray<Entity> players = playersGroup.ToEntityArray(Allocator.TempJob, out JobHandle handle1);
		NativeArray<Translation> positions = playersGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle handle2);
		inputDependencies = JobHandle.CombineDependencies(inputDependencies, handle1, handle2);

		var job = new FindClosestPlayerJob()
		{
			players = players,
			positions = positions,
		};

		JobHandle handle = job.Schedule(this, inputDependencies);

		return handle;
	}
}