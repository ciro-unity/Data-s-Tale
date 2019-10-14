using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class EnemyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	[SerializeField]
	private float speed = .1f;
	public int initialHealth = 50;
	public float seekRange = 5;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(EnemyTag));
		dstManager.AddComponentData(entity, new MovementInput { MoveAmount = new float3() });
		dstManager.AddComponentData(entity, new Speed { Value = speed });
		dstManager.AddComponentData(entity, new AttackInput { Attack = false });
		dstManager.AddComponentData(entity, new Target { Value = Entity.Null });
		dstManager.AddComponentData(entity, new AlertRange { RangeSq = seekRange * seekRange });
		dstManager.AddComponentData(entity, new Health { Current = initialHealth, FullHealth = initialHealth });
    }
}
