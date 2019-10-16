using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class EnemyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	public float speed = .1f;
	public int initialHealth = 50;
	public float seekRange = 10f;
	public float attackRange = 2f;
	
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

        dstManager.AddComponent(entity, typeof(EnemyTag));
		dstManager.AddComponent(entity, typeof(AnimationState));
		dstManager.AddComponent(entity, typeof(CopyTransformToGameObject)); //will sync MB Transform and ECS Transform

		dstManager.AddComponentData(entity, new MovementInput { MoveAmount = new float3() });
		dstManager.AddComponentData(entity, new Speed { Value = speed });
		dstManager.AddComponentData(entity, new AttackInput { Attack = false });
		dstManager.AddComponentData(entity, new Target { Entity = Entity.Null });
		dstManager.AddComponentData(entity, new AlertRange { Range = seekRange });
		dstManager.AddComponentData(entity, new AttackRange { Range = attackRange } );
		dstManager.AddComponentData(entity, new Health { Current = initialHealth, FullHealth = initialHealth });
    }
}
