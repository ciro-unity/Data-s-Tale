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
		
		for(int i=0; i<entities.Length; i++)
		{
			bool targetFound = false;
			Entity target = Entity.Null;

			//check the squared distance to this Player and see if it's under the closest one we already found
			float currentSqDistance = math.lengthsq(positions[i].Value - translation.Value);
			if(currentSqDistance < closestSqDistance)
			{
				target = entities[i];
				closestSqDistance = currentSqDistance;
				targetFound = true;
			}
			
			if(targetFound)
				ECB.AddComponent<Target>(entityIndex, entity, new Target{ Entity = entities[i] });
		}
	}


	struct CheckIfTargetInRangeJob : IJobForEachWithEntity<Target, AlertRange, Translation, MovementInput>
	{
		[ReadOnly] public ComponentDataFromEntity<Translation> targetTranslations;
		public EntityCommandBuffer.Concurrent ECB;

		public void Execute(Entity entity, int index,
							ref Target target,
							[ReadOnly] ref AlertRange alertRange,
							[ReadOnly] ref Translation translation,
							ref MovementInput movementInput)
		{
			float distanceSquared = math.lengthsq(targetTranslations[target.Entity].Value - translation.Value);
			if(distanceSquared > alertRange.Range * alertRange.Range)
			{
				//target is too far, remove component
				ECB.RemoveComponent<Target>(index, entity);
				movementInput.MoveAmount = float3.zero; //stop the entity
			}
		}
	}
    
	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		//shared ECB between all jobs
		EntityCommandBuffer.Concurrent ecb = endInitECBSystem.CreateCommandBuffer().ToConcurrent();

		//all enemies finding the closest player
		NativeArray<Entity> players = playersGroup.ToEntityArray(Allocator.TempJob, out JobHandle handle1);
		NativeArray<Translation> playerPositions = playersGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle handle2);
		inputDependencies = JobHandle.CombineDependencies(inputDependencies, handle1, handle2);

		var job1 = new FindClosestPlayerJob()
		{
			players = players,
			positions = playerPositions,
			ECB = ecb,
		};

		JobHandle job1Handle = job1.Schedule(this, inputDependencies);


		//all players finding the closest enemy
		NativeArray<Entity> enemies = enemyGroup.ToEntityArray(Allocator.TempJob, out JobHandle handle3);
		NativeArray<Translation> enemyPositions = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle handle4);
		job1Handle = JobHandle.CombineDependencies(job1Handle, handle3, handle4);

		var job2 = new FindClosestEnemyJob()
		{
			enemies = enemies,
			positions = enemyPositions,
			ECB = ecb,
		};

		JobHandle job2Handle = job2.Schedule(this, job1Handle);
		endInitECBSystem.AddJobHandleForProducer(job2Handle);


		//all units, check if the target is still in range
		var job3 = new CheckIfTargetInRangeJob()
		{
			ECB = ecb,
			targetTranslations = GetComponentDataFromEntity<Translation>(true),
		};
		JobHandle job3Handle = job3.Schedule(this, job2Handle);
		endInitECBSystem.AddJobHandleForProducer(job3Handle);

		return job3Handle;
	}
}