using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public float timePerFaze = 90;
    public float faze = 0;
    bool isShrinking;
    public float minPos = 0;
    public float maxPos = 512;
    public float divider = 2;
    bool isEnd;
    static int minPlayers = 1;
    public void ZoneLoop()
    {
        while (true)
        {
            Vector3 end = new Vector3(Random.Range(minPos, maxPos), transform.position.y, Random.Range(minPos, maxPos));

            if (isShrinking == true && transform.localScale.x > 0)
            {
                transform.position = Vector3.Lerp(transform.position, end, 1 / timePerFaze * Time.fixedDeltaTime);
                transform.localScale = new Vector3(
                transform.localScale.x - 1 / divider * Time.fixedDeltaTime, 1,
                transform.localScale.z - 1 / divider * Time.fixedDeltaTime);
            }
            if (transform.localScale.x < 0.0001f)
            {
                isEnd = true;
                transform.localScale = new Vector3(0, 0, 0);
            }
        }
    }
    IEnumerator ShrinkingMethod()
    {
        faze += minPlayers;
        isShrinking = true;
        yield return new WaitForSeconds(timePerFaze);
        isShrinking = false;
        if (isEnd == false)
        {
            StartCoroutine(PauseMethod());
        }
    }
    IEnumerator PauseMethod()
    {
        faze += minPlayers;
        yield return new WaitForSeconds(timePerFaze);
        StartCoroutine(ShrinkingMethod());
    }
    
}
