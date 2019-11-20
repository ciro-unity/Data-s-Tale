using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class CollectableAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	public int scoreValue = 1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
		dstManager.AddComponentData(entity, new Collectable{ Value = scoreValue });
	}
}
