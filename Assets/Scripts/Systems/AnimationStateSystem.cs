using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

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

            animationState = new AnimationState
			{
				Speed = animationMult,
				IsWalking = movementSqMagnitude > 0f,
				TriggerAttack = attackInput.Attack == true,
				TriggerTakeDamage = false, //TODO: hardcoded for now, wire it in
			};
        }
    }

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
        var job = new AnimationStateJob();
		JobHandle handle1 = job.Schedule(this, inputDependencies);

		var job2 = new SimpleAnimationStateJob();
		JobHandle handle2 = job2.Schedule(this, handle1);

        return handle2;
    }
}