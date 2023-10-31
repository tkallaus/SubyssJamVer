using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class bigLogicScript : MonoBehaviour
{
    public Transform waterRising;
    public GameObject[] tentaclees;
    public tApproach[] tentacleesWarnings;
    private float[] tentacleApproachTimers;
    public float spawnTimer = 5f;

    private int[] randomTSelection;

    public static float floodProgress = 0f;
    public static bool youLose = false;

    public Vector3 lWaterStartPos;
    public Vector3 lWaterEndPos;

    public static Stopwatch speedrunTimer;
    public UnityEngine.UI.Text finalTime;
    private bool timeAdded = false;

    void Start()
    {
        tentacleApproachTimers = new float[4];
        randomTSelection = new int[4];
        for (int i = 0; i < 4; i++)
        {
            tentacleApproachTimers[i] = 10f;
            randomTSelection[i] = Random.Range(0, 3);
        }

        speedrunTimer = new Stopwatch();
        speedrunTimer.Start();
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if(spawnTimer < 0)
        {
            int rand = Random.Range(0, 4);
            if(!tentacleesWarnings[rand].approaching && !tentacleesWarnings[rand].tentacleReady)
            {
                tentacleesWarnings[rand].resetPos();
                tentacleesWarnings[rand].activate(Random.Range(.75f, 3f));
                spawnTimer = 6f;
            }
        }
        for(int i = 0; i < tentacleesWarnings.Length; i++)
        {
            if (tentacleesWarnings[i].tentacleReady)
            {
                tentaclees[i * 3 + randomTSelection[i]].SetActive(true);
                floodProgress += 1.2f * Time.deltaTime;
            }
        }
        if(floodProgress >= 100f)
        {
            youLose = true;
            finalTime.gameObject.SetActive(true);
            if (!timeAdded)
            {
                finalTime.text += speedrunTimer.Elapsed.ToString("hh\\:mm\\:ss\\.ff");
                timeAdded = true;
            }
        }
        else
        {
            finalTime.gameObject.SetActive(false);
            timeAdded = false;
        }
    }
    private void FixedUpdate()
    {
        waterRising.position = Vector3.Lerp(lWaterStartPos, lWaterEndPos, floodProgress/100f);
    }

    public void resetWarning(int num)
    {
        tentacleesWarnings[num].tentacleReady = false;
        tentacleesWarnings[num].resetPos();
        randomTSelection[num] = Random.Range(0, 3);
    }
}
