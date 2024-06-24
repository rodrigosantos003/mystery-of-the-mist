using System;
using Cinemachine;
using TMPro;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField]
    private float speed = 10;
    
    [SerializeField]
    private float rotationSpeed = 100;
    
    [SerializeField]
    private float edgeScrollSize = 20;
    
    [SerializeField]
    private bool useEdgeScrolling = true;
    
    [SerializeField]
    private bool useDragPan = true;
    
    [SerializeField]
    private float dragPanSpeed = 2;
    
    [SerializeField]
    private float fieldOfViewMax = 50;
    
    [SerializeField]
    private float fieldOfViewMin = 10;
    
    [SerializeField]
    private float zoomSpeed = 5;
    
    [SerializeField]
    private BoxCollider boxCollider;
    private Bounds bounds;
    
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    private bool dragPanMoveActive = false;
    private Vector2 lastMousePosition = Vector2.zero;
    private float targetFieldOfView;

    private void Start()
    {
        bounds = boxCollider.bounds;
        targetFieldOfView = fieldOfViewMax;
    }
    private void Update()
    {
        HandleCameraMovement();
        
        if(useEdgeScrolling) HandleCameraMovementEdgeScrolling();
        
        if(useDragPan) HandleCameraMovementDragPan();
        
        HandleCameraRotation();
        
        HandleCameraZoom();
    }

    private void HandleCameraZoom()
    {
        targetFieldOfView -= 5 * Input.mouseScrollDelta.y;
        targetFieldOfView = Mathf.Clamp(targetFieldOfView, fieldOfViewMin, fieldOfViewMax);
        
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFieldOfView, Time.deltaTime * zoomSpeed);
    }

    private void HandleCameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);
        
        inputDir.z = Input.GetAxisRaw("Vertical");
        inputDir.x = Input.GetAxisRaw("Horizontal");
        
        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        
        Vector3 newPosition = transform.position + moveDir * (Time.deltaTime * speed);
        
        if(IsWithinBounds(newPosition)) transform.position = newPosition;
    }

    private void HandleCameraMovementEdgeScrolling()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if(Input.mousePosition.x < edgeScrollSize)                  inputDir.x = -1;
        if(Input.mousePosition.x > Screen.width - edgeScrollSize)   inputDir.x = 1;
        if(Input.mousePosition.y < edgeScrollSize)                  inputDir.z = -1;
        if(Input.mousePosition.y > Screen.height - edgeScrollSize)  inputDir.z = 1;
        
        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        
        Vector3 newPosition = transform.position + moveDir * (Time.deltaTime * speed);
        
        if(IsWithinBounds(newPosition)) transform.position = newPosition;
    }
    
    private void HandleCameraMovementDragPan()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);
        if (Input.GetMouseButtonDown(1))
        {
            dragPanMoveActive = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1)) dragPanMoveActive = false;

        if (dragPanMoveActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;
            
            mouseMovementDelta *= -1;
            
            inputDir.x = mouseMovementDelta.x * dragPanSpeed;
            inputDir.z = mouseMovementDelta.y * dragPanSpeed;
            
            lastMousePosition = Input.mousePosition;
        }
        
        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        
        Vector3 newPosition = transform.position + moveDir * (Time.deltaTime * speed);
        
        if(IsWithinBounds(newPosition)) transform.position = newPosition;
    }
    private void HandleCameraRotation()
    {
        float rotateDir = 0f;
        
        if (Input.GetKey(KeyCode.Q)) rotateDir = -1;
        if (Input.GetKey(KeyCode.E)) rotateDir = 1;

        transform.eulerAngles += new Vector3(0, rotateDir * rotationSpeed * Time.deltaTime, 0);
    }
    
    private bool IsWithinBounds(Vector3 newPosition)
    {
        return bounds.Contains(newPosition);
    }
}