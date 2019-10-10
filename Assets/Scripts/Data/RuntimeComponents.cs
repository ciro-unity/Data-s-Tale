using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PlayerTag : IComponentData { }

[Serializable]
public struct EnemyTag : IComponentData { }

[Serializable]
public struct Movement : IComponentData
{
	public float3 MoveAmount;
}

[Serializable]
public struct Speed : IComponentData
{
	public float Value;
}

[Serializable]
public struct Attack : IComponentData
{
	public bool IsAttacking;
}

// [Serializable]
// public struct HumanPlayerInput : IComponentData
// {
// 	public float2 MovementInput;
// 	public bool Attack;
// }

