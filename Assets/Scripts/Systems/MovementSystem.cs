using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

//This systems takes care of moving characters around, applying a force to physics Entities based on their MovementInput
public class MovementSystem : JobComponentSystem
{
    [BurstCompile]
    struct MovementJob : IJobForEach<MovementInput, Speed, Translation, PhysicsVelocity, PhysicsMass, Rotation>
    {
        public void Execute([ReadOnly] ref MovementInput movement,
							[ReadOnly] ref Speed speed,
							ref Translation translation,
							ref PhysicsVelocity physicsVelocity,
							ref PhysicsMass physicsMass,
							ref Rotation rotation)
        {
			//Reset the movement on the Y axis
			float3 newTranslation = translation.Value;
			newTranslation.y = 0f;
			translation.Value = newTranslation;

			//Assign velocity to the physics body
			physicsVelocity.Linear = movement.MoveAmount * speed.Value * .5f;
			physicsMass.InverseInertia = new float3(0,1,0); //lock rotation on X and Z

			//Force rotation to the direction of movement
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