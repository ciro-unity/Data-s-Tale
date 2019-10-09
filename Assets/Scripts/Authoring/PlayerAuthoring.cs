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
		Movement movement = World.Active.EntityManager.GetComponentData<Movement>(entityReference);
		
		float squareMagnitude = math.lengthsq(movement.MoveAmount);
		float animationMult = math.min(squareMagnitude + .1f, 1f);
		
		animator.SetFloat("Speed", animationMult);
		if(squareMagnitude > 0f)
		{
			animator.SetBool("IsWalking", true);
		}
		else
		{
			animator.SetBool("IsWalking", false);
		}

		Attack attack = World.Active.EntityManager.GetComponentData<Attack>(entityReference);
		if(attack.IsAttacking)
		{
			Debug.Log("Attacked	");
			animator.SetTrigger("Attack");
			World.Active.EntityManager.SetComponentData<Attack>(entityReference, new Attack{IsAttacking = false}); //reset it to false
		}
	}

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
		entityReference = conversionSystem.GetPrimaryEntity(this.transform); //save a reference to the entity for syncing animations

        dstManager.AddComponent(entity, typeof(PlayerTag));
		dstManager.AddComponentData(entity, new Movement { MoveAmount = new float2(), SpeedMultiplier = speed  } );
		dstManager.AddComponentData(entity, new Attack {IsAttacking = false});
		dstManager.AddComponent(entity, typeof(CopyTransformToGameObject)); //will sync MB Transform and ECS Transform
    }
}
