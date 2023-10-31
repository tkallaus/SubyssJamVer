using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterEffectScript : MonoBehaviour
{
    public GameObject waterEffect;
    public Transform waterRisingRef;

    void Update()
    {
        if(transform.position.y < waterRisingRef.position.y + 6)
        {
            waterEffect.SetActive(true);
        }
        else
        {
            waterEffect.SetActive(false);
        }
    }
}
