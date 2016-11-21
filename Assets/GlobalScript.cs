using UnityEngine;
using System.Collections;
using System;
using Vuforia;

public class GlobalScript : MonoBehaviour, ITrackableEventHandler {

    public Camera camera;
    public GameObject imageTarget;
    public TrackableBehaviour mTrackableBehaviour;
    public Transform[] teapots;
    public GameObject ghostObject;

    private bool isTracked;
    private int pickedUp = -1;
    private bool isFree;
    private int freeCount;

    private int freeThresh = 20;
    private float distThresh = 0.5f;
    private float epsilonDist = 0.1f;

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

        foreach(Touch touch in Input.touches)
        {
            if(touch.phase == TouchPhase.Began)
            {
                RaycastHit hitObj;
                Ray ray = camera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out hitObj))
                {
                    if (pickedUp == -1)
                    {
                        for (int i = 0; i < teapots.Length; i++)
                        {
                            if (hitObj.transform.parent == teapots[i].transform)
                            {
                                pickedUp = i;
                                pickUp(pickedUp, 0);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (hitObj.transform == imageTarget.transform)
                        {
                            putDown(pickedUp,
                                teapots[pickedUp].transform.rotation.eulerAngles.y,
                                hitObj.point.x,
                                hitObj.point.z);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < teapots.Length; i++)
        {
            float distance = Vector3.Distance(ghostObject.transform.position, teapots[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                minIndex = i;
            }
        }

        if (pickedUp == -1)
        {
            if (isFree)
            {
                if (isTracked && minDistance < epsilonDist)
                {
                    pickedUp = minIndex;
                    pickUp(pickedUp, teapots[pickedUp].transform.rotation.eulerAngles.y - ghostObject.transform.rotation.eulerAngles.y - 90);
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
                    putDown(pickedUp, 
                        teapots[pickedUp].transform.rotation.eulerAngles.y,
                        teapots[pickedUp].transform.position.x,
                        teapots[pickedUp].transform.position.z);
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

    private void pickUp(int index, float yRot)
    {
        isFree = false;
        freeCount = 0;

        teapots[pickedUp].transform.parent = camera.transform;
        teapots[pickedUp].transform.localPosition = new Vector3(0.0f, 0.0f, distThresh);
        teapots[pickedUp].transform.localRotation =
            Quaternion.Euler(
                yRot,
                90,
                -90);
        teapots[pickedUp].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        teapots[pickedUp].gameObject.GetComponentsInChildren<MeshRenderer>()[0].enabled = true;
    }

    private void putDown(int index, float yRot, float xPos, float zPos)
    {
        teapots[pickedUp].transform.parent = imageTarget.transform;
        teapots[pickedUp].transform.localPosition = new Vector3(xPos, 0.0f, zPos);
        teapots[pickedUp].transform.localRotation = Quaternion.Euler(
            0,
            yRot,
            0);
        teapots[pickedUp].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        teapots[pickedUp].gameObject.GetComponentsInChildren<MeshRenderer>()[0].enabled = isTracked;

        pickedUp = -1;
        isFree = false;
        freeCount = 0;
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
