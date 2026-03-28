using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCarCamera : MonoBehaviour
{
    private Transform playerCarTransform;
    private Transform cameraPointTransform;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure car has 'Player' tag.");
            return;
        }

        playerCarTransform = player.transform;

        Transform camPoint = playerCarTransform.Find("camera point");

        if (camPoint == null)
        {
            Debug.LogError("camera point not found! Check name in hierarchy.");
            return;
        }

        cameraPointTransform = camPoint;
    }

    // ✅ Use LateUpdate instead of FixedUpdate
    void LateUpdate()
    {
        if (playerCarTransform == null || cameraPointTransform == null) return;

        // Smooth follow
        transform.position = Vector3.SmoothDamp(
            transform.position,
            cameraPointTransform.position,
            ref velocity,
            0.1f   // smoother and stable
        );

        // Smooth rotation (better than direct LookAt)
        Quaternion targetRotation = Quaternion.LookRotation(
            playerCarTransform.position - transform.position
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            5f * Time.deltaTime
        );
    }
}