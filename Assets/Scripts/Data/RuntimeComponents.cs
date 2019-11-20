using System;
using Unity.Entities;
using Unity.Mathematics;


//----------------------------------  PLAYER-SPECIFIC ----------------------------------------

[Serializable]
public struct PlayerTag : IComponentData { }

public struct Score : IComponentData
{
	public int Value;
}

//----------------------------------  ALL CHARACTERS ----------------------------------------

[Serializable]
public struct EnemyTag : IComponentData { }

[Serializable]
public struct Wanderer : IComponentData
{
	public Unity.Mathematics.Random RandomSeed;
	public float3 InitialPosition;
}

[Serializable]
public struct MovementInput : IComponentData
{
	public float3 MoveAmount;
}

[Serializable]
public struct AttackInput : IComponentData
{
	public bool Attack;
	public float AttackLength;
	public int AttackStrength;
}

[Serializable]
public struct Speed : IComponentData
{
	public float Value;
}

[Serializable]
public struct AttackRange : IComponentData
{
	public float Range;
}

[Serializable]
public struct AlertRange : IComponentData
{
	public float Range;
}

[Serializable]
public struct AnimationState : ISystemStateComponentData
{
	public float Speed;
	public bool IsWalking;
	public bool TriggerAttack;
	public bool TriggerTakeDamage;
	public bool TriggerIsDead;
}

[Serializable]
public struct DealBlow : IComponentData
{
	public float When;
	public int DamageAmount;
}


//----------------------------------  ATTACKING/DAMAGING ----------------------------------------

[Serializable]
public struct Target : IComponentData
{
	public Entity Entity;
}

[Serializable]
public struct Damage : IBufferElementData
{
	public int Amount;
}

[Serializable]
public struct Health : IComponentData
{
	public int Current;
	public int FullHealth;
}

[Serializable]
public struct IsDead : IComponentData { }

//----------------------------------  ITEMS ----------------------------------------

public struct Collectable : IComponentData
{
	public int Value;
}


//----------------------------------  GENERAL USE ----------------------------------------

[Serializable]
public struct Busy : IComponentData
{
	public float Until; //time value
}