using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("General Enemy Parameters")]
    [SerializeField] int pointForDead = 1;
    [SerializeField] GameObject AttackEffectPrefab;
    [SerializeField] GameObject DeathEffectPrefab;

    public bool IsAlive { get; set; }

    protected PlayerController player;

    protected bool IsAttackingActive = true;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();
    }

    protected virtual void Die() { }
}
