using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class MovementSystem : JobComponentSystem
{
    [BurstCompile]
	[ExcludeComponent(typeof(Busy))]
    struct MovementSystemJob : IJobForEach<MovementInput, Speed, PhysicsVelocity, PhysicsMass, Rotation>
    {
     	public float fixedDeltaTime;
        
        public void Execute([ReadOnly] ref MovementInput movement,
							[ReadOnly] ref Speed speed,
							ref PhysicsVelocity physicsVelocity,
							ref PhysicsMass physicsMass,
							ref Rotation rotation)
        {            
			//Assign velocity
			physicsVelocity.Linear = movement.MoveAmount / fixedDeltaTime * speed.Value * .01f;
			physicsMass.InverseInertia = new float3(0,0,0); //lock rotation on X and Z

			//Force rotation
			if(!physicsVelocity.Linear.Equals(float3.zero))
			{
				float3 heading = math.normalize(physicsVelocity.Linear);
				rotation.Value = quaternion.LookRotation(heading, math.up());
			}
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MovementSystemJob();
		job.fixedDeltaTime = Time.fixedDeltaTime;
        return job.Schedule(this, inputDependencies);
    }
}