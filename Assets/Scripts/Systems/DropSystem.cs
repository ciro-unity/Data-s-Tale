using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//This system instantiates a piece of loot (Droppable) when an object/character is killed/destroyed
//It finds objects that are dead, but haven't been destroyed yet
[UpdateBefore(typeof(DeathSystem))]
public class DropSystem : JobComponentSystem
{
	public EndSimulationEntityCommandBufferSystem endSimECBSystem;

	protected override void OnCreate()
	{
		endSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	[RequireComponentTag(typeof(IsDead))]
    struct SpawnDropJob : IJobForEachWithEntity<Translation, Droppable>
    {
		public EntityCommandBuffer.Concurrent ECB;
		public float currentTime;

        public void Execute(Entity entity, int jobIndex,
							[ReadOnly] ref Translation translation,
							[ReadOnly] ref Droppable droppable)
        {
			//Spawn the droppable, and remove this object
            Entity e = ECB.Instantiate(jobIndex, droppable.Drop);
			float3 dropPos = translation.Value + new float3(0f, 0.8f, 0f);
			ECB.SetComponent<Translation>(jobIndex, e, new Translation{ Value = dropPos }); //position it on the entity that spawned it
			ECB.AddComponent<Busy>(jobIndex, e, new Busy{ Until = currentTime + .5f });

			ECB.RemoveComponent<Droppable>(jobIndex, entity);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		EntityCommandBuffer.Concurrent ecb = endSimECBSystem.CreateCommandBuffer().ToConcurrent();

        var job = new SpawnDropJob()
		{
			ECB = ecb,
			currentTime = Time.time,
		};
        
        return job.Schedule(this, inputDependencies);
    }
}