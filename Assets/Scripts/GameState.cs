using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    int zero = 0;
    int secToEnd = 60;
    IEnumerator CountDown()
    {
        if(zero < secToEnd)
        {
            secToEnd -= 1;
            yield return new WaitForSeconds(secToEnd);
            StartCoroutine(CountDown());
        }
        else
        {
            secToEnd = 60;
        }
    }
}
