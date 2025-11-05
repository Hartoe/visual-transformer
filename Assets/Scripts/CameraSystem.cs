using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;

    [Header("Movement Speeds")]
    [SerializeField] float moveSpeed;
    [SerializeField] float zoomSpeed;
    [SerializeField] float rotateSpeed;

    [Header("Movement Bounds")]
    [SerializeField] bool enableEdgeScroll = false;
    [SerializeField] bool enableDragControls = false;
    [SerializeField] float minZoom;
    [SerializeField] float maxZoom;

    [Header("Input Action References")]
    [SerializeField] InputActionReference dragPan;
    [SerializeField] InputActionReference dragRoll;
    [SerializeField] InputActionReference mousePosition;
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference rotateLeft;
    [SerializeField] InputActionReference rotateRight;
    [SerializeField] InputActionReference zoom;

    Vector3 inputDirection;
    Vector2 currentMousePosition, lastMousePosition;
    float targetFOV = 50;

    private void Start()
    {
        // Setup Dragging Events
        dragPan.action.started += DragStart;
        dragRoll.action.started += DragStart;
    }

    private void Update()
    {
        // Get input vectors
        currentMousePosition = mousePosition.action.ReadValue<Vector2>();
        inputDirection = move.action.ReadValue<Vector2>();

        HandleCameraMovement();
        HandleCameraRotation();
        HandleCameraZoom();
    }

    private void HandleCameraMovement()
    {
        // Handle edge scrolling
        if (enableEdgeScroll) HandleEdgeScroll();

        if (enableDragControls) HandleDragPan();

        // Update position
        Vector3 moveDirection = transform.forward * inputDirection.y + transform.right * inputDirection.x;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
    
    private void HandleCameraRotation()
    {
        float rotateDirection = 0f;

        if (enableDragControls) rotateDirection = HandleDragRoll(rotateDirection);

        // Update rotation
        if (rotateRight.action.IsPressed()) rotateDirection = -rotateSpeed;
        if (rotateLeft.action.IsPressed()) rotateDirection = rotateSpeed;
        transform.eulerAngles += new Vector3(0, rotateDirection * Time.deltaTime, 0);
    }

    private void HandleDragPan()
    {
        // Calculate Mouse Delta from Drag Pan
        if (dragPan.action.IsPressed())
        {
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            inputDirection.x = -mouseDelta.x;
            inputDirection.y = -mouseDelta.y;
            lastMousePosition = currentMousePosition;
        }
    }

    private void HandleEdgeScroll()
    {
        int edgeScrollSize = 20;

        if (currentMousePosition.x < edgeScrollSize) inputDirection.x = -1f;
        if (currentMousePosition.y < edgeScrollSize) inputDirection.y = -1f;
        if (currentMousePosition.x > Screen.width - edgeScrollSize) inputDirection.x = 1f;
        if (currentMousePosition.y > Screen.height - edgeScrollSize) inputDirection.y = 1f;
    }

    private float HandleDragRoll(float rotateDirection)
    {
        // Calculate Mouse Delta from Drag Roll
        if (dragRoll.action.IsPressed())
        {
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            rotateDirection = mouseDelta.x * rotateSpeed;
            lastMousePosition = currentMousePosition;
        }

        return rotateDirection;
    }
    
    private void HandleCameraZoom()
    {
        Vector2 zoomDirection = zoom.action.ReadValue<Vector2>().normalized;
        targetFOV += -zoomDirection.y * zoomSpeed;
        targetFOV = Mathf.Clamp(targetFOV, minZoom, maxZoom);
        cinemachineVirtualCamera.m_Lens.FieldOfView =
            Mathf.Lerp(cinemachineVirtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    private void DragStart(InputAction.CallbackContext context)
    {
        lastMousePosition = currentMousePosition;
    }
}
