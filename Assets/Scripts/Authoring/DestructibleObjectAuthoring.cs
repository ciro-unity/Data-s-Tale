using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class DestructibleObjectAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public int health = 10;
	public GameObject drop;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(EnemyTag));
        dstManager.AddComponentData(entity, new Health { Current = health, FullHealth = health } );
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
