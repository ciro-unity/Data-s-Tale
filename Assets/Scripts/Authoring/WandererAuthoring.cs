using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class WandererAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	public float speed = 2;
	
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
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		entityReference = conversionSystem.GetPrimaryEntity(this.transform); //save a reference to the entity for syncing animations

		dstManager.AddComponent(entity, typeof(AnimationState));
		dstManager.AddComponent(entity, typeof(CopyTransformToGameObject)); //will sync MB Transform and ECS Transform

		uint seed = System.Convert.ToUInt32(UnityEngine.Random.Range(0, 10000));
		dstManager.AddComponentData(entity, new Wanderer { RandomSeed = new Unity.Mathematics.Random(seed) } );
		dstManager.AddComponentData(entity, new MovementInput { MoveAmount = new float3()} );
		dstManager.AddComponentData(entity, new Speed { Value = speed } );
	}
}
