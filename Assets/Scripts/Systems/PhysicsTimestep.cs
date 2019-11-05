using Unity.Entities;
using UnityEngine;
using Unity.Physics.Systems;
 
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class PrePhysicsSetDeltaTimeSystem : ComponentSystem
{
    public bool isRealTimeStep = true;
    public float timeScale = 1;
    public float previousDeltaTime = Time.fixedDeltaTime;
 
    protected override void OnUpdate()
    {
        previousDeltaTime = Time.fixedDeltaTime;
 
        if (isRealTimeStep)
            Time.fixedDeltaTime = Time.deltaTime * timeScale;
        else
            Time.fixedDeltaTime = Time.fixedDeltaTime * timeScale;
    }
}
 
[UpdateAfter(typeof(ExportPhysicsWorld))]
public class PostPhysicsResetDeltaTimeSystem : ComponentSystem
{
    public PrePhysicsSetDeltaTimeSystem preSystem;
 
    protected override void OnCreate()
    {
        preSystem = World.Active.GetOrCreateSystem<PrePhysicsSetDeltaTimeSystem>();
    }
 
    protected override void OnUpdate()
    {
        Time.fixedDeltaTime = preSystem.previousDeltaTime;
    }
}