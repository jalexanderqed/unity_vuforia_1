using UnityEngine;
using System.Collections;
using System;
using Vuforia;

public class GlobalScript : MonoBehaviour, ITrackableEventHandler {

    public Camera camera;
    public Transform imageTarget;
    public TrackableBehaviour mTrackableBehaviour;
    public Transform[] teapots;

    private bool isTracked;
    private int pickedUp = -1;
    private bool isFree;
    private int freeCount;

    private int freeThresh = 20;
    private float distThresh = 0.8f;

    // Use this for initialization
    void Start()
    {
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }
        isTracked = false;
    }

    // Update is called once per frame
    void Update () {
        float minDistance = 1e10f;
        int minIndex = 0;

        for (int i = 0; i < teapots.Length; i++)
        {
            float distance = Vector3.Distance(camera.transform.position, teapots[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                minIndex = i;
            }
        }
        
        Debug.Log("pickedUp: " + pickedUp);

        if (pickedUp == -1)
        {
            if (isFree)
            {
                if (isTracked && minDistance < distThresh)
                {
                    pickedUp = minIndex;
                    isFree = false;
                    freeCount = 0;

                    teapots[pickedUp].transform.parent = camera.transform;
                    teapots[pickedUp].transform.localPosition = new Vector3(0.0f, 0.0f, 0.8f);
                    teapots[pickedUp].transform.localRotation = Quaternion.Euler(0, 90, -90);
                    teapots[pickedUp].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    teapots[pickedUp].gameObject.GetComponentsInChildren<MeshRenderer>()[0].enabled = true;
                }
            }
            else
            {
                if (minDistance > distThresh)
                {
                    freeCount++;
                    if (freeCount > freeThresh) isFree = true;
                }
            }
        }
        else
        {
            if (isFree)
            {
                if (isTracked && camera.transform.position.y < distThresh)
                {
                    teapots[pickedUp].transform.parent = imageTarget.transform;
                    teapots[pickedUp].transform.localPosition = new Vector3(0.4f, 0.0f, 0.0f);
                    teapots[pickedUp].transform.localRotation = Quaternion.Euler(0, 0, 0);
                    teapots[pickedUp].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    teapots[pickedUp].gameObject.GetComponentsInChildren<MeshRenderer>()[0].enabled = isTracked;

                    pickedUp = -1;
                    isFree = false;
                    freeCount = 0;
                }
            }
            else
            {
                if (camera.transform.position.y > distThresh)
                {
                    freeCount++;
                    if (freeCount > freeThresh) isFree = true;
                }
            }
        }
	}

    public void OnTrackableStateChanged(
                                TrackableBehaviour.Status previousStatus,
                                TrackableBehaviour.Status newStatus)
    {
        isTracked = (newStatus == TrackableBehaviour.Status.DETECTED ||
             newStatus == TrackableBehaviour.Status.TRACKED ||
             newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED);
    }
}
