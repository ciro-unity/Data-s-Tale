using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;

[UpdateBefore(typeof(MovementSystem))]
public class PlayerInputSystem : ComponentSystem, Player1InputActions.IPlayerActions
{
	private Player1InputActions player1Input;
	private float2 movementInput;
	private bool attackInput;

	protected override void OnCreate()
	{
		player1Input = new Player1InputActions();

	#if UNITY_STANDALONE || UNITY_EDITOR
		player1Input.bindingMask = InputBinding.MaskByGroup(player1Input.KeyboardMouseScheme.bindingGroup);
	#elif UNITY_IOS || UNITY_ANDROID
		player1Input.bindingMask = InputBinding.MaskByGroup(player1Input.TouchScheme.bindingGroup);
	#endif

		player1Input.Player.SetCallbacks(this);
		player1Input.Player.Enable();
	}

	protected override void OnDestroy()
	{
		player1Input.Player.Disable();
	}

    protected override void OnUpdate()
    {
		Entities.ForEach((ref Movement movement) =>
		{		
			//Pass the values to the ECS component
			movement = new Movement
			{
				MoveAmount = movementInput,
				SpeedMultiplier = movement.SpeedMultiplier,
			};
		});

		if(attackInput)
		{
			Entities.ForEach((ref Attack attack) =>
			{		
				//Pass the values to the ECS component
				attack = new Attack
				{
					IsAttacking = true,
				};
			});

			attackInput = false;
		}
    }

	public void OnMove(InputAction.CallbackContext context)
	{
		movementInput = context.action.ReadValue<Vector2>();
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		attackInput = context.performed;
	}

	public void OnUnused(InputAction.CallbackContext context)
	{
	}
}