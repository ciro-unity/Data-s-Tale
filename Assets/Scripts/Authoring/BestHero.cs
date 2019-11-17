using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class BestHero : MonoBehaviour, IConvertGameObjectToEntity
{
    public float speed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(PlayerTag));
		
		dstManager.AddComponentData(entity, new MovementInput { MoveAmount = new float3()} );
		dstManager.AddComponentData(entity, new Speed { Value = speed } );
		dstManager.AddComponentData(entity, new AttackInput { Attack = false, AttackLength = 1f, AttackStrength = 3 });
    }
}
