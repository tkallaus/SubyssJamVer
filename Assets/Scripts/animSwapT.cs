using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animSwapT : MonoBehaviour
{
    private Animation anim;
    public bool slapped = false;
    public GameObject tentacleModel;
    public int tentacleWarningNum;

    private void OnEnable()
    {
        anim = GetComponent<Animation>(); //should be in start to be efficient prolly but onEnable goes before start :<
        anim.PlayQueued("Wriggle");
        slapped = false;
    }
    private void Update()
    {
        if (slapped)
        {
            anim.CrossFade("Exit");
            slapped = false;
        }
        if (!anim.isPlaying)
        {
            tentacleModel.SetActive(false);
        }
    }
    private void OnDisable()
    {
        tentacleModel.SetActive(true);
        FindObjectOfType<bigLogicScript>().resetWarning(tentacleWarningNum);
        slapped = false;
    }
}
