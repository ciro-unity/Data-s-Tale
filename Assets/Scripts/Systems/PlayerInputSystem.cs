using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;

//This system acts as a bridge between the InputSystem (Unity classic) and the ECS world,
//by reading input and storing it in the OnUpdate into two ECS components: AttackInput and MovementInput.
//The value of these inputs is picked up by other ECS systems
[UpdateInGroup(typeof(InitializationSystemGroup))]
[AlwaysUpdateSystem]
public class PlayerInputSystem : ComponentSystem, Player1InputActions.IPlayerActions
{
	private Player1InputActions player1Input;
	private Vector2 movementInput;
	private bool attackInput;

	//Input private variables
	private Vector2 cursorInitialPosition;
	private bool cursorPressed;

	protected override void OnCreate()
	{
		//Need to enable this one custom composite
		InputSystem.RegisterBindingComposite<UnityEngine.InputSystem.Composites.Vector2WithOneModifier>();

		player1Input = new Player1InputActions();

		//Enabling the right set of bindings according to the platform
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
		//Pass the values to the ECS component on the player entity, only if the player is not "busy" (Busy component is present)
		Entities.WithNone<Busy>().ForEach((ref MovementInput movement, ref AttackInput atk, ref PlayerTag playerTag) =>
		{		
			Vector3 movement3 = new Vector3(movementInput.x, 0f, movementInput.y);
			
			movement.MoveAmount = movement3;
			atk.Attack = attackInput;
		});

		//Regardless of whether the player is busy or not, we clear the attack input.
		//This way once it becomes free, it won't attack again as a result of a tap done during the busy phase.
		attackInput = false;
    }


	//----------------------------------  INPUT SYSTEM LISTENERS ----------------------------------------

	public void OnMove(InputAction.CallbackContext context)
	{
		movementInput = context.action.ReadValue<Vector2>();
	}

	public void OnPointerMove(InputAction.CallbackContext context)
	{
		//Debug.Log("OnPointerMove " + context.action.ReadValue<Vector2>());

		switch (context.phase)
		{
			case InputActionPhase.Started:
				cursorInitialPosition = context.action.ReadValue<Vector2>();
				break;

			case InputActionPhase.Performed:
				movementInput = math.normalizesafe((context.action.ReadValue<Vector2>() - cursorInitialPosition) * .1f);
				break;

			case InputActionPhase.Canceled:
				movementInput = Vector2.zero;
				break;
		}
	}

	public void OnTouchMove(InputAction.CallbackContext context)
	{
		//Debug.Log("OnTouchMove " + context.action.ReadValue<Vector2>());
		
		OnPointerMove(context);
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		//Debug.Log("OnFire " + context.performed);
		if(context.phase == InputActionPhase.Performed)
		{
			attackInput = context.performed;
		}
	}
}