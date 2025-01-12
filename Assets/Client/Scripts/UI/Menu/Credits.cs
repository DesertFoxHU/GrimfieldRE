using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    private bool IsActive = false;
    private Vector3 DefaultPos;
    public GameObject[] HideOnStart;
    public GameObject ActualObject;
    public float time;
    private float DefaultTime;
    public float speed;

    public void Start()
    {
        DefaultPos = ActualObject.transform.position;
        DefaultTime = time;
    }

    public void ShowCredits()
    {
        IsActive = true;
        foreach(GameObject go in HideOnStart)
        {
            go.SetActive(false);
        }
    }

    public void Reset()
    {
        IsActive = false;
        foreach (GameObject go in HideOnStart)
        {
            go.SetActive(true);
        }
        ActualObject.transform.position = DefaultPos;
        time = DefaultTime;
    }

    public void Update()
    {
        if (!IsActive) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IsActive = false;
            Reset();
            return;
        }

        FindAnyObjectByType<CameraRandom>().isTowardsRight = true;
        ActualObject.transform.position = Vector3.Lerp(
                ActualObject.transform.position,
                new Vector3(ActualObject.transform.position.x - 1f, ActualObject.transform.position.y, ActualObject.transform.position.z),
                Time.deltaTime * speed);

        time -= Time.deltaTime;
        if(time <= 0)
        {
            Reset();
        }
    }
}
