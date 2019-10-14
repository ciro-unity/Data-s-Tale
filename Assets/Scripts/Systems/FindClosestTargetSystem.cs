using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class FindClosestTargetSystem : JobComponentSystem
{
	public EntityQuery playersGroup;

	protected override void OnCreate()
	{
		playersGroup = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<Translation>());
		
		RequireForUpdate(playersGroup);
	}

	//[BurstCompile]
	[RequireComponentTag(typeof(EnemyTag))]
	struct FindClosestTargetJob : IJobForEach<Target, Translation, AlertRange>
	{
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Entity> players;
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Translation> positions;

		public void Execute(ref Target target,
							[ReadOnly] ref Translation translation,
							[ReadOnly] ref AlertRange alertRange)
		{
			float closestSqDistance = alertRange.RangeSq;
			for(int i=0; i<players.Length; i++)
			{
				//check the squared distance to this Player and see if it's under the closest one we already found
				float currentSqDistance = math.lengthsq(positions[i].Value - translation.Value);
				if(currentSqDistance < closestSqDistance)
				{
					target.Value = players[i];
					closestSqDistance = currentSqDistance;
				}
			}
		}
	}
    
	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		playersGroup.AddDependency(inputDependencies);

		NativeArray<Entity> players = playersGroup.ToEntityArray(Allocator.TempJob, out JobHandle handle1);
		NativeArray<Translation> positions = playersGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle handle2);
		inputDependencies = JobHandle.CombineDependencies(handle1, handle2);

		var job = new FindClosestTargetJob()
		{
			players = players,
			positions = positions,
		};

		JobHandle handle = job.Schedule(this, inputDependencies);

		return handle;
	}
}