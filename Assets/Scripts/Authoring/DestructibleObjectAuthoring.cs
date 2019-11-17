using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class DestructibleObjectAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	public int health = 10;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(EnemyTag));
        dstManager.AddComponentData(entity, new Health { Current = health, FullHealth = health } );
		dstManager.AddBuffer<Damage>(entity);
        
    }
}
