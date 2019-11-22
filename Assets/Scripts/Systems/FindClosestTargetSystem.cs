using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class FindClosestTargetSystem : JobComponentSystem
{
	public EntityQuery playersGroup;
	public EntityQuery enemyGroup;
	public EndInitializationEntityCommandBufferSystem endInitECBSystem;

	protected override void OnCreate()
	{
		playersGroup = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<Translation>(), ComponentType.Exclude<IsDead>());
		enemyGroup = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>(), ComponentType.ReadOnly<Translation>(), ComponentType.Exclude<IsDead>());
		
		RequireForUpdate(playersGroup);
		RequireForUpdate(enemyGroup);

		endInitECBSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
	}


	[RequireComponentTag(typeof(EnemyTag))]
	[ExcludeComponent(typeof(Target))]
	struct FindClosestPlayerJob : IJobForEachWithEntity<Translation, AlertRange>
	{
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Entity> players;
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Translation> positions;
		public EntityCommandBuffer.Concurrent ECB;

		public void Execute(Entity entity, int entityIndex,
							[ReadOnly] ref Translation translation,
							[ReadOnly] ref AlertRange alertRange)
		{
			EvaluateList(translation, alertRange, players, positions, entity, entityIndex, ECB);
		}
	}


	[RequireComponentTag(typeof(PlayerTag))]
	[ExcludeComponent(typeof(Target))]
	struct FindClosestEnemyJob : IJobForEachWithEntity<Translation, AlertRange>
	{
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Entity> enemies;
		[DeallocateOnJobCompletion]
		[ReadOnly] public NativeArray<Translation> positions;
		public EntityCommandBuffer.Concurrent ECB;

		public void Execute(Entity entity, int entityIndex,
							[ReadOnly] ref Translation translation,
							[ReadOnly] ref AlertRange alertRange)
		{
			EvaluateList(translation, alertRange, enemies, positions, entity, entityIndex, ECB);
		}
	}


	public static void EvaluateList(Translation translation,
									AlertRange alertRange,
									NativeArray<Entity> entities,
									NativeArray<Translation> positions,
									Entity entity,
									int entityIndex,
									EntityCommandBuffer.Concurrent ECB)
	{
		float closestSqDistance = alertRange.Range * alertRange.Range;
		
		bool targetFound = false;
		Entity target = Entity.Null;

		for(int i=0; i<entities.Length; i++)
		{
			//check the squared distance to this Player and see if it's under the closest one we already found
			float currentSqDistance = math.lengthsq(positions[i].Value - translation.Value);
			if(currentSqDistance < closestSqDistance)
			{
				target = entities[i];
				closestSqDistance = currentSqDistance;
				targetFound = true;
			}	
		}

		if(targetFound)
			ECB.AddComponent<Target>(entityIndex, entity, new Target{ Entity = target });
	}

	//This job checks if an Entity linked in Target has become invalid for some reason (destroyed, or has moved away).
	//Excluding the DealBlow component means that an attacking Entity which is in the process of landing an attack won't lose its Target.
	[ExcludeComponent(typeof(DealBlow))]
	struct CheckIfTargetValidJob : IJobForEachWithEntity<Target, AlertRange, Translation, MovementInput>
	{
		[ReadOnly] public ComponentDataFromEntity<Translation> targetTranslations;
		public EntityCommandBuffer.Concurrent ECB;

		public void Execute(Entity entity, int index,
							ref Target target,
							[ReadOnly] ref AlertRange alertRange,
							[ReadOnly] ref Translation translation,
							ref MovementInput movementInput)
		{
			//First we check if the Entity exists in the Translation data array.
			//If not, it's dead and can be removed as the target
			if(!targetTranslations.Exists(target.Entity))
			{
				ECB.RemoveComponent<Target>(index, entity);
				movementInput.MoveAmount = float3.zero; //stops the entity
				return;
			}

			//Then, we check the distance. If beyond the alertRange, the entity is too far
			//In this case, an enemy would go into idle
			float distanceSquared = math.lengthsq(targetTranslations[target.Entity].Value - translation.Value);
			if(distanceSquared > alertRange.Range * alertRange.Range)
			{
				ECB.RemoveComponent<Target>(index, entity);
				movementInput.MoveAmount = float3.zero; //stops the entity
			}
		}
	}
    
	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		//shared ECB between all jobs
		EntityCommandBuffer.Concurrent ecb = endInitECBSystem.CreateCommandBuffer().ToConcurrent();

		//Job 1
		//First we run through all the enemies finding the closest player (generally there's only one).
		//We prepare 2 NativeArrays: a reference to the players, and to their positions. This way enemies can run distance checks and note down which Entity is the closest player.
		NativeArray<Entity> players = playersGroup.ToEntityArray(Allocator.TempJob, out JobHandle handle1);
		NativeArray<Translation> playerPositions = playersGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle handle2);
		
		//This code is running on the main thread but the 2 NativeArrays above are fetched in a job.
		//This is why we combine the dependencies and pass them all to the job when we schedule it (see below), to ensure that all the data is ready at the moment the job is launched.
		//For more info: https://gametorrahod.com/minimum-main-thread-block-with-out-jobhandle-overload/ (first section)
		JobHandle newInputDependencies = JobHandle.CombineDependencies(inputDependencies, handle1, handle2);

		var job1 = new FindClosestPlayerJob()
		{
			players = players,
			positions = playerPositions,
			ECB = ecb,
		};

		JobHandle job1Handle = job1.Schedule(this, newInputDependencies);

		//Job 2
		//Now iterating through the players (only one) finding the closest enemy.
		//We prepare the NativeArrays like in the job above.
		NativeArray<Entity> enemies = enemyGroup.ToEntityArray(Allocator.TempJob, out JobHandle handle3);
		NativeArray<Translation> enemyPositions = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle handle4);
		
		//Again, we combine dependencies to make sure that job1 is run as well as the fetching of the 2 NativeArrays, before running job2
		JobHandle job1Dependencies = JobHandle.CombineDependencies(job1Handle, handle3, handle4);

		var job2 = new FindClosestEnemyJob()
		{
			enemies = enemies,
			positions = enemyPositions,
			ECB = ecb,
		};

		JobHandle job2Handle = job2.Schedule(this, job1Dependencies);
		endInitECBSystem.AddJobHandleForProducer(job2Handle);

		//Job 3
		//All characters already with a Target but not currently dealing an attack, check if the target is still in range or dead.
		var job3 = new CheckIfTargetValidJob()
		{
			ECB = ecb,
			targetTranslations = GetComponentDataFromEntity<Translation>(true),
		};
		JobHandle job3Handle = job3.Schedule(this, job2Handle);
		endInitECBSystem.AddJobHandleForProducer(job3Handle);

		return job3Handle;
	}
}