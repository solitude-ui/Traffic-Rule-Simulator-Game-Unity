using UnityEngine;
using System;

public class PathManager : MonoBehaviour
{
    [Header("Path Setup")]
    [SerializeField] private Transform[] waypoints;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private int currentIndex;
    private ArrowIndicator activeArrow;

    public bool IsPathComplete => HasValidPath() && currentIndex >= waypoints.Length;
    public event Action PathCompleted;

    private void Start()
    {
        if (!HasValidPath())
        {
            Debug.LogWarning("PathManager: Assign at least 1 waypoint. Add an ArrowIndicator child under each waypoint you want to collect.", this);
            return;
        }

        DisableAllWaypointArrows();
        ShowCurrentArrow();
    }

    public void OnArrowCollected()
    {
        currentIndex++;

        if (currentIndex < waypoints.Length)
        {
            ShowCurrentArrow();
            return;
        }

        HideActiveArrow();
        Debug.Log("PathManager: Goal reached!");
        PathCompleted?.Invoke();
    }

    public bool IsPlayerCollider(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag(playerTag)) return true;
        if (other.transform.root.CompareTag(playerTag)) return true;

        Rigidbody attachedRigidbody = other.attachedRigidbody;
        if (attachedRigidbody != null && attachedRigidbody.CompareTag(playerTag))
            return true;

        return false;
    }

    private void ShowCurrentArrow()
    {
        if (!HasCurrentWaypoint()) return;

        HideActiveArrow();

        activeArrow = waypoints[currentIndex].GetComponentInChildren<ArrowIndicator>(true);
        if (activeArrow == null)
        {
            Debug.LogWarning($"PathManager: Waypoint '{waypoints[currentIndex].name}' does not have an ArrowIndicator child.", waypoints[currentIndex]);
            return;
        }

        activeArrow.Init(this);
        activeArrow.gameObject.SetActive(true);
    }

    private bool HasValidPath()
    {
        return waypoints != null && waypoints.Length >= 1;
    }

    private bool HasCurrentWaypoint()
    {
        if (!HasValidPath()) return false;

        if (currentIndex >= waypoints.Length)
        {
            Debug.LogWarning("PathManager: No more waypoint arrows left to activate.", this);
            return false;
        }

        if (waypoints[currentIndex] == null)
        {
            Debug.LogWarning("PathManager: A waypoint reference is missing.", this);
            return false;
        }

        return true;
    }

    private void DisableAllWaypointArrows()
    {
        if (waypoints == null) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            ArrowIndicator arrow = waypoints[i].GetComponentInChildren<ArrowIndicator>(true);
            if (arrow != null)
                arrow.gameObject.SetActive(false);
        }
    }

    private void HideActiveArrow()
    {
        if (activeArrow == null) return;

        activeArrow.gameObject.SetActive(false);
        activeArrow = null;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;

            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);
        }

        Transform finalWaypoint = waypoints[waypoints.Length - 1];
        if (finalWaypoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(finalWaypoint.position, 0.4f);
    }
}
