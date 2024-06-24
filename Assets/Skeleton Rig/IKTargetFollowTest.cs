using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Created by Ashley Neall and Pranav Wagh
// When we implement this, we can no longer move the IK Target to move the body part, and instead we use the cube (which our MR user will interact with!)

public class IKTargetFollowTest : MonoBehaviour
{
    [SerializeField] GameObject CubeTarget; // the interactable cube

    // Start is called before the first frame update
    void Start()
    {
        CubeTarget.transform.position = transform.position;
        CubeTarget.transform.rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = CubeTarget.transform.position;
        transform.rotation = CubeTarget.transform.rotation;
    }
}