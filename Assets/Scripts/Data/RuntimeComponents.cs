﻿using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PlayerTag : IComponentData { }

[Serializable]
public struct EnemyTag : IComponentData { }

[Serializable]
public struct MovementInput : IComponentData
{
	public float3 MoveAmount;
}

[Serializable]
public struct AttackInput : IComponentData
{
	public bool Attack;
}

[Serializable]
public struct Target : IComponentData
{
	public Entity Value;
}

[Serializable]
public struct AnimationState : IComponentData
{
	public float Speed;
	public bool IsWalking;
	public bool TriggerAttack;
	public bool TriggerTakeDamage;
}

[Serializable]
public struct Busy : IComponentData
{
	public float Until; //time value
}

[Serializable]
public struct Speed : IComponentData
{
	public float Value;
}

[Serializable]
public struct AlertRange : IComponentData
{
	public float RangeSq;
}

[Serializable]
public struct Health : IComponentData
{
	public int Current;
	public int FullHealth;
}

[Serializable]
public struct IsDead : IComponentData { }