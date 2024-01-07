using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ReticleBehaviour : MonoBehaviour
{
    public GameObject Child;
    public DrivingSurfaceManager DrivingSurfaceManager;
    public ARPlane CurrentPlane;

    // Start is called before the first frame update
    private void Start()
    {
        if (transform.childCount > 0)
        {
            Child = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("No child object found for the reticle.");
        }

        if (DrivingSurfaceManager == null)
        {
            Debug.LogError("DrivingSurfaceManager is not set in the inspector.");
        }
    }

    private void Update()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main camera is null.");
            return;
        }

        if (DrivingSurfaceManager == null)
        {
            Debug.LogError("DrivingSurfaceManager is null.");
            return;
        }

        if (DrivingSurfaceManager.RaycastManager == null)
        {
            Debug.LogError("RaycastManager is null.");
            return;
        }

        // Conduct a ray cast to position this object.
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

        var hits = new List<ARRaycastHit>();
        if (DrivingSurfaceManager.RaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds))
        {
            Debug.Log("Raycast hit");
            ARRaycastHit? hit = null;

            // If you don't have a locked plane already...
            var lockedPlane = DrivingSurfaceManager.LockedPlane;
            if (lockedPlane == null)
            {
                Debug.Log("lockedPlane is null, using first hit in `hits`.");
                hit = hits[0]; // Safe to use hits[0] since we've checked hits.Count > 0
            }
            else if (hits.Any(x => x.trackableId == lockedPlane.trackableId))
            {
                hit = hits.SingleOrDefault(x => x.trackableId == lockedPlane.trackableId);
            }
            else
            {
                Debug.Log("No hit corresponds to the locked plane's trackableId");
            }

            if (hit.HasValue)
            {
                if (DrivingSurfaceManager.PlaneManager == null)
                {
                    Debug.LogError("PlaneManager is null.");
                    return;
                }

                CurrentPlane = DrivingSurfaceManager.PlaneManager.GetPlane(hit.Value.trackableId);
                if (CurrentPlane == null)
                {
                    Debug.LogError("GetPlane returned null.");
                    return;
                }

                // Move this reticle to the location of the hit.
                transform.position = hit.Value.pose.position;
                Debug.Log("Moving reticle to position: " + hit.Value.pose.position);
            }
            else
            {
                Debug.Log("Hit has no value");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit");
        }

        if (Child == null)
        {
            Debug.LogError("Child is null.");
            return;
        }
        Child.SetActive(CurrentPlane != null);
    }
}
