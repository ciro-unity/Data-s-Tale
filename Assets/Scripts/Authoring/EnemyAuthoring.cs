using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class EnemyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	[SerializeField]
	private float speed = .1f;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(EnemyTag));
		dstManager.AddComponentData(entity, new Movement { MoveAmount = new float3()} );
		dstManager.AddComponentData(entity, new Speed { Value = speed} );
		dstManager.AddComponentData(entity, new Attack {IsAttacking = false});
    }
}
