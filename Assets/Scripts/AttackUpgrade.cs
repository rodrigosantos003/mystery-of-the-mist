using System;
using UnityEngine;

[Serializable]
public class AttackUpgrade
{
    [SerializeField] private float attackMultiplier;
    [SerializeField] private float rangeMultiplier;
    
    public float AttackMultiplier => attackMultiplier;
    public float RangeMultiplier => rangeMultiplier;
}