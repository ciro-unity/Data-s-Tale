using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

//This system takes care of building the AnimationState data from the inputs (MovementInput, AttackInput)
//which is then passed to the 'classic' Animator component on the connected GameObject to drive animation.
//The sync-up is performed in the Update of each Authoring script (see PlayerAuthoring, EnemyAuthoring, etc.)
[UpdateAfter(typeof(AttackSystem))]
[UpdateAfter(typeof(MovementSystem))]
public class AnimationStateSystem : JobComponentSystem
{
    [BurstCompile]
    struct AnimationStateJob : IJobForEach<AnimationState, MovementInput, AttackInput>
    {

        public void Execute(ref AnimationState animationState,
							[ReadOnly] ref MovementInput movementInput,
							[ReadOnly] ref AttackInput attackInput)
        {
			float movementSqMagnitude = math.lengthsq(movementInput.MoveAmount);
			float animationMult = math.min(movementSqMagnitude + .1f, 1f);

			//All the data inside AnimationState is used to set the parameters of the Animator (floats, bool and triggers)
            animationState = new AnimationState
			{
				Speed = animationMult,
				IsWalking = movementSqMagnitude > 0f,
				TriggerAttack = attackInput.Attack == true,
				TriggerTakeDamage = false, //TODO: hardcoded for now, wire it in
			};
        }
    }

	//Variation of the above job for Entities that don't have an attack animation
	[BurstCompile]
	[ExcludeComponent(typeof(AttackInput))]
    struct SimpleAnimationStateJob : IJobForEach<AnimationState, MovementInput>
    {
        
        public void Execute(ref AnimationState animationState,
							[ReadOnly] ref MovementInput movementInput)
        {
			float movementSqMagnitude = math.lengthsq(movementInput.MoveAmount);
			float animationMult = math.min(movementSqMagnitude + .1f, 1f);

            animationState = new AnimationState
			{
				Speed = animationMult,
				IsWalking = movementSqMagnitude > 0f
			};
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
		//Job 1
		//ALl characters that can attack
        var job = new AnimationStateJob();
		JobHandle handle1 = job.Schedule(this, inputDependencies);

		//Job 2
		//All non-attacking characters
		var job2 = new SimpleAnimationStateJob();
		JobHandle handle2 = job2.Schedule(this, handle1);

        return handle2;
    }
}