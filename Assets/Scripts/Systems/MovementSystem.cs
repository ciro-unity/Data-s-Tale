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
	//[ExcludeComponent(typeof(Busy))]
    struct MovementJob : IJobForEach<MovementInput, Speed, Translation, PhysicsVelocity, PhysicsMass, Rotation>
    {
        public void Execute([ReadOnly] ref MovementInput movement,
							[ReadOnly] ref Speed speed,
							ref Translation translation,
							ref PhysicsVelocity physicsVelocity,
							ref PhysicsMass physicsMass,
							ref Rotation rotation)
        {
			//Lock the movement on the Y axis
			float3 newTranslation = translation.Value;
			newTranslation.y = 0f;
			translation.Value = newTranslation;

			//Assign velocity
			physicsVelocity.Linear = movement.MoveAmount * speed.Value * .5f;
			physicsMass.InverseInertia = new float3(0,1,0); //lock rotation on X and Z

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
        var job = new MovementJob();
        return job.Schedule(this, inputDependencies);
    }
}