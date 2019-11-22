using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class EnemyAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public float speed = .1f;
	public int initialHealth = 50;
	public int attackStrength = 2;
	public float seekRange = 10f;
	public float attackRange = 2f;
	public AnimationClip attackClip;
	public GameObject drop;
	
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
		if(animState.TriggerIsDead) animator.SetTrigger("IsDead");

		if(animState.TriggerIsDead) this.enabled = false;
	}

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
		entityReference = conversionSystem.GetPrimaryEntity(this.transform); //save a reference to the entity for syncing animations

        dstManager.AddComponent(entity, typeof(EnemyTag));
		dstManager.AddComponent(entity, typeof(AnimationState));
		dstManager.AddComponent(entity, typeof(CopyTransformToGameObject)); //will sync MB Transform and ECS Transform

		dstManager.AddComponentData(entity, new MovementInput { MoveAmount = new float3() });
		dstManager.AddComponentData(entity, new Speed { Value = speed });
		float atkAnimLength = attackClip.length; //extract the length of the animationClip so we can make the entity Busy later on
		dstManager.AddComponentData(entity, new AttackInput { Attack = false, AttackLength = atkAnimLength, AttackStrength = attackStrength });
		dstManager.AddComponentData(entity, new AlertRange { Range = seekRange });
		dstManager.AddComponentData(entity, new AttackRange { Range = attackRange } );
		dstManager.AddComponentData(entity, new Health { Current = initialHealth, FullHealth = initialHealth });
		dstManager.AddBuffer<Damage>(entity);
		if(drop != null) dstManager.AddComponentData<Droppable>(entity, new Droppable{ Drop = conversionSystem.GetPrimaryEntity(drop) });
    }

	//This function will "subscribe" the gameObject (a Prefab) into a conversion mechanism
	//which allows, during ECS gameplay, to instantiate the entity that was created out of this Prefab during conversion
	public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
	{
		gameObjects.Add(drop);
	}
}
