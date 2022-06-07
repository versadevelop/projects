using System;
using System.Collections;
using System.Collections.Generic;
using Tears_Of_Void.Control;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    // Input variables
    KeyCode leftMouse = KeyCode.Mouse0;
    KeyCode rightMouse = KeyCode.Mouse1;
    KeyCode middleMouse = KeyCode.Mouse2;


    // Camera Variables
    public float cameraHeight = 1.75f;
    public float cameraMaxDistance = 25f;
    float cameraMaxTilt = 90f;
    [Range(0, 4)] public float cameraSpeed = 2f;
    float currentPan; // Pan = rotation on Y axis
    float currentTilt = 10f; // Pan = rotation on X axis
    float currentDistance = 5f;
    [HideInInspector] public bool autoRunReset = false;

    // Camera smoothing
    [SerializeField]
    float panAngle, panOffset;
    bool camXAdjust, camYAdjust;
    float rotationXCushion = 3f;
    float rotationXSpeed = 0f;
    float yRotMin = 0f;
    float yRotMax = 20f;
    float rotationYSpeed = 0f;

    // CamState
    public CameraStates cameraState = CameraStates.CameraNone;

    // Options
    [Range(0.25f, 1.75f)] public float cameraAdjustSpeed = 1f;
    public CameraMoveState camMoveState = CameraMoveState.OnlyWhileMoving;

    // References
    PlayerControls player;
    public Transform tilt;
    Camera mainCam;

    void Start()
    {
        player = FindObjectOfType<PlayerControls>();
        player.mainCamera = this;
        mainCam = Camera.main;

        transform.position = player.transform.position + Vector3.up * cameraHeight;
        transform.rotation = player.transform.rotation;

        tilt.eulerAngles = new Vector3(currentTilt, transform.eulerAngles.y, transform.eulerAngles.z);
        mainCam.transform.position += tilt.forward * -currentDistance;
    }

    void Update()
    {
        if(InteractWithUI()) return;
        if (!Input.GetKey(leftMouse) && !Input.GetKey(rightMouse) && !Input.GetKey(middleMouse)) // No mouse button is pressed
        {
            cameraState = CameraStates.CameraNone;
        }
        else if (Input.GetKey(leftMouse) && !Input.GetKey(rightMouse) && !Input.GetKey(middleMouse)) // If LMB is pressed
        {
            cameraState = CameraStates.CameraRotate;
        }
        else if (!Input.GetKey(leftMouse) && Input.GetKey(rightMouse) && !Input.GetKey(middleMouse)) // If RMB is pressed
        {
            cameraState = CameraStates.CameraSteer;
        }
        else if ((Input.GetKey(leftMouse) && Input.GetKey(rightMouse)) || Input.GetKey(middleMouse)) // If LMB & RMB or MMB is pressed
        {
            cameraState = CameraStates.CameraRun;
        }

            CameraInputs();
    }

    private bool InteractWithUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    void LateUpdate()
    {
        panAngle = Vector3.SignedAngle(transform.forward, player.transform.forward, Vector3.up); // Returns a signed (pos or neg) angle up to 180°

        switch(camMoveState)
        {
            case CameraMoveState.OnlyWhileMoving:
                if (player.inputNormalized.magnitude > 0 || player.rotation != 0)
                {
                    CameraXAdjust();
                    CameraYAdjust();
                }
                break;

            case CameraMoveState.OnlyHorizontalWhileMoving:
                if (player.inputNormalized.magnitude > 0 || player.rotation != 0)
                {
                    CameraXAdjust();
                }
                break;

            case CameraMoveState.AlwaysAdjust:
                CameraXAdjust();
                CameraYAdjust();
                break;

            case CameraMoveState.NeverAdjust:
                CameraNeverAdjust();
                break;
        }


        CameraTransforms();
    }

    void CameraInputs()
    {
        if (cameraState != CameraStates.CameraNone)
        {
            if(!camYAdjust && (camMoveState == CameraMoveState.AlwaysAdjust || camMoveState == CameraMoveState.OnlyWhileMoving))
            {
                camYAdjust = true;
            }

            if (cameraState == CameraStates.CameraRotate)
            {
                if (!camXAdjust && camMoveState != CameraMoveState.NeverAdjust)
                {
                    camXAdjust = true;
                }

                if (player.steer)
                {
                    player.steer = false;
                }

                currentPan += Input.GetAxis("Mouse X") * cameraSpeed;
            }
            else if (cameraState == CameraStates.CameraSteer || cameraState == CameraStates.CameraRun)
            {
                if(!player.steer) // When we rotate the camera and then steer, the player will face the same direction as the camera
                {
                    Vector3 playerReset = player.transform.eulerAngles;
                    playerReset.y = transform.eulerAngles.y;

                    player.transform.eulerAngles = playerReset;

                    player.steer = true;
                }
            }

            currentTilt -= Input.GetAxis("Mouse Y") * cameraSpeed;
            currentTilt = Mathf.Clamp(currentTilt, -cameraMaxTilt, cameraMaxTilt);
        }
        else
        {
            if (player.steer)
            {
                player.steer = false;
            }
        }

        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * 2.5f;
        currentDistance = Mathf.Clamp(currentDistance, 0, cameraMaxDistance);
    }

    void CameraXAdjust() // Horizontal-only adjustment
    {
        if (cameraState != CameraStates.CameraRotate)
        {
            if (camXAdjust)
            {
                rotationXSpeed += (Time.deltaTime /2) * cameraAdjustSpeed;

                if (Mathf.Abs(panAngle) > rotationXCushion)
                {
                    currentPan = Mathf.Lerp(currentPan, currentPan + panAngle, rotationXSpeed);
                }
                else
                {
                    camXAdjust = false;
                }
                
            }
            else
            {
                if (rotationXSpeed > 0)
                {
                    rotationXSpeed = 0;
                }
                currentPan = player.transform.eulerAngles.y;
            }
        }
    }

    void CameraYAdjust()
    {
        if (cameraState == CameraStates.CameraNone)
        {
            if (camYAdjust)
            {
                rotationYSpeed += (Time.deltaTime / 3) * cameraAdjustSpeed;

                if (currentTilt >= yRotMax || currentTilt <= yRotMin)
                {
                    currentTilt = Mathf.Lerp(currentTilt, yRotMax / 2, rotationYSpeed);
                }
                else if (currentTilt < yRotMax && currentTilt > yRotMin)
                {
                    camYAdjust = false;
                }
            }
            else
            {
                if (rotationYSpeed > 0)
                {
                    rotationYSpeed = 0;
                }
            }
        }
    }

    void CameraNeverAdjust()
    {
        switch (cameraState)
        {
            case CameraStates.CameraSteer:
            case CameraStates.CameraRun:
                if (panOffset != 0)
                {
                    panOffset = 0;
                }

                currentPan = player.transform.eulerAngles.y;
                break;
            case CameraStates.CameraNone:
                currentPan = player.transform.eulerAngles.y - panOffset;
                break;
            case CameraStates.CameraRotate:
                panOffset = panAngle;
                break;
        }
    }

    void CameraTransforms()
    {
        if (cameraState == CameraStates.CameraRun)
        {
            player.autoRun = true;
            
            if (!autoRunReset)
            {
                autoRunReset = true;
            }
        }
        else
        {
            if (autoRunReset)
            {
                player.autoRun = false;
                autoRunReset = false;
            }
        }

        transform.position = player.transform.position + Vector3.up * cameraHeight;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentPan, transform.eulerAngles.z);
        tilt.eulerAngles = new Vector3(currentTilt, tilt.eulerAngles.y, tilt.eulerAngles.z);
        mainCam.transform.position = transform.position + tilt.forward * -currentDistance;
    }

    public enum CameraStates { CameraNone, CameraRotate, CameraSteer, CameraRun }

    public enum CameraMoveState { OnlyWhileMoving, OnlyHorizontalWhileMoving, AlwaysAdjust, NeverAdjust }
}
