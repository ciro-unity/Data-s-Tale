using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Physics;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PlayerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float speed = 1f;
	
	private Entity entityReference;
	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		AnimationState animState = World.Active.EntityManager.GetComponentData<AnimationState>(entityReference);
	
		//transfer the values to the Animator state machine
		animator.SetFloat("Speed", animState.Speed);
		animator.SetBool("IsWalking", animState.IsWalking);
		if(animState.TriggerAttack)	animator.SetTrigger("Attack");
		if(animState.TriggerTakeDamage) animator.SetTrigger("TakeDamage");
	}

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
		entityReference = conversionSystem.GetPrimaryEntity(this.transform); //save a reference to the entity for syncing animations

        dstManager.AddComponent(entity, typeof(PlayerTag));
		dstManager.AddComponentData(entity, new MovementInput { MoveAmount = new float3()} );
		dstManager.AddComponentData(entity, new Speed { Value = speed } );
		dstManager.AddComponentData(entity, new AttackInput { Attack = false });
		dstManager.AddComponent(entity, typeof(AnimationState));
		dstManager.AddComponent(entity, typeof(CopyTransformToGameObject)); //will sync MB Transform and ECS Transform
    }
}
