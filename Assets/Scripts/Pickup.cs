using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Pickup Type")]
    [SerializeField] Pickups type;
    private enum Pickups { Coin, DoublePoints, Watch, Shield}

    [Header("Coins Parameters")]
    [SerializeField] int coinValue = 1;
    [SerializeField] int coinPoints = 1;
    [SerializeField] AudioClip coinSound;

    [Header("Watch Parameters")]
    [Range(0.1f, 0.3f)][SerializeField] float slowTimeValue;

    private void OnTriggerEnter(Collider other)
    {
        if (type == Pickups.Coin) 
        {
            GameManager.AddScore(coinPoints);
            GameManager.AddCoins(coinValue);
            Destroy(gameObject);
        }
        else if (type == Pickups.Watch) 
        { 
        
        }
        else if (type == Pickups.DoublePoints) 
        { 
        
        }
        else if (type == Pickups.Shield) 
        { 
        
        }
    }
}
