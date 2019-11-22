using Unity.Entities;
using UnityEngine;
using Unity.Physics.Systems;
 
//Temporary hack systems to fix the lack of FixedUpdate in ECS
//Credits to Ivan 'Nothke' Notaroš for the hack

//This system sets the fixedDeltaTime to the value of deltaTime so
//the ECS physics (which are using fixedDeltaTime anyway) can be in sync with the ECS loop.
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class PrePhysicsSetDeltaTimeSystem : ComponentSystem
{
    public bool isRealTimeStep = true; //Change this to false to disable the hack
    public float timeScale = 1;
    public float originalFixedDeltaTime = Time.fixedDeltaTime;
 
    protected override void OnUpdate()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
 
        if (isRealTimeStep
			&& Time.frameCount > 5)
            Time.fixedDeltaTime = Time.deltaTime * timeScale;
        else
            Time.fixedDeltaTime = Time.fixedDeltaTime * timeScale;
    }
}
 
//This system just puts the fixedDeltaTime back to its original value
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
        Time.fixedDeltaTime = preSystem.originalFixedDeltaTime;
    }
}