using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public int playerCount = 0;
    public int secondsTo = 60;
    string waitingForPlayers = " seconds till the game starts.";
    string waitingForZoneShrink = " seconds till the zone starts moving";
    string waitingForZonePause = " seconds till the zone stops moving";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        Server.Start(50, 26950);
        StartCoroutine(CountingDown());
    }
    private void OnApplicationQuit()
    {
        Server.Stop();
    }
    public Player InstantiatePlayer()
    {
        playerCount += 1;
        return Instantiate(playerPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Player>();
    }
    public IEnumerator CountingDown()
    {
        if (playerCount >= minPlayers)
        {
            secondsTo = 10;
            while (true)
            {
                if (secondsTo >= 1 && playerCount >= minPlayers)
                {
                    yield return new WaitForSeconds(1);
                    secondsTo -= 1;
                    ServerSend.GameTimer(secondsTo, waitingForPlayers);
                }
                else if (secondsTo >= 1)
                {
                    yield return new WaitForSeconds(1);
                    StartCoroutine(CountingDown());
                    break;
                }
                else
                {
                    StartCoroutine(ZoneLoop());
                    break;
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(CountingDown());
        }
    }
    // ZONE
    public Transform ZoneObject;
    public Transform InstatiateObj;
    public GameObject[] guns;
    public float timePerFaze = 90;
    public float faze = 0;
    bool isShrinking;
    public float minPos = -512;
    public float maxPos = 512;
    public float divider = 2;
    bool zoneClosedIn;
    public int minPlayers = 2;
    public float zoneShrinker = 0.001f;
    public bool isInGame = false;
    public IEnumerator ZoneLoop()
    {
        isInGame = true;
        // Set max players so no more can join
        Server.MaxPlayers = playerCount;
        // Starting methods
        OnePlayerLeft();
        StartCoroutine(ShrinkingMethod());
        // Getting values for zone
        Vector3 end = new Vector3(Random.Range(minPos, maxPos), ZoneObject.position.y, Random.Range(minPos, maxPos));
        float zoneShrinkHalf = zoneShrinker / 2;
        // Zone loop
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            if (isShrinking == true && ZoneObject.localScale.x > zoneShrinkHalf)
            {
                ZoneObject.position = Vector3.Lerp(ZoneObject.position, end, 1 / timePerFaze * Time.fixedDeltaTime);
                ZoneObject.localScale = new Vector3(
                ZoneObject.localScale.x - zoneShrinker, 1,
                ZoneObject.localScale.z - zoneShrinker);
                ServerSend.ZoneValues(ZoneObject.position, ZoneObject.localScale.x);
            }
            else if (transform.localScale.x <= zoneShrinker)
            {
                zoneClosedIn = true;
                ZoneObject.localScale = new Vector3(0, 0, 0);
                ServerSend.GameTimer(-1, "Zone has closed in!");
                break;
            }
        }
    }
    IEnumerator ShrinkingMethod()
    {
        isShrinking = true;
        StartCoroutine(TimeToEnd(timePerFaze, waitingForZonePause));
        yield return new WaitForSeconds(timePerFaze);
        isShrinking = false;
        if (zoneClosedIn == false)
        {
            StartCoroutine(PauseMethod());
        }
    }
    IEnumerator PauseMethod()
    {
        StartCoroutine(TimeToEnd(timePerFaze, waitingForZoneShrink));
        yield return new WaitForSeconds(timePerFaze);
        StartCoroutine(ShrinkingMethod());
    }
    IEnumerator TimeToEnd(float timeToEnd, string message)
    {
        while (true)
        {
            if (timeToEnd >= 1 && !zoneClosedIn)
            {
                yield return new WaitForSeconds(1);
                timeToEnd -= 1;
                ServerSend.GameTimer((int)timeToEnd, message);
            }
            else
            {
                break;
            }
        }
    }
    IEnumerator OnePlayerLeft()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (playerCount == 1)
            {
                isInGame = false;
                ServerSend.Placement(playerCount);
                // kick everybody, so the server is empty
                yield return new WaitForSeconds(5);
                foreach (Client _client in Server.clients.Values)
                {
                    _client.player.Died();
                }
                // reset
                yield return new WaitForSeconds(5);
                ResetValues();
                StartCoroutine(CountingDown());
                Server.MaxPlayers = 50;
                break;
            }
        }
    }
    void ResetValues()
    {
        ZoneObject.position = new Vector3(0, 0, 0);
        ZoneObject.localScale = new Vector3(1, 1, 1);
        ServerSend.ZoneValues(ZoneObject.position, 1);
        foreach (Client _client in Server.clients.Values)
        {
            _client.player.ResetValues();
        }
    }
}
