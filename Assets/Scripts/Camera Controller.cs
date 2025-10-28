using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] float movespeed = 2f;
    [SerializeField] float zoomspeed = 3f;
    [SerializeField] float rotatespeed = 1.5f;

    [Header("Movement Clamps")]
    [SerializeField] float minZoom = -5f;
    [SerializeField] float maxZoom = -150f;

    [Header("Input References")]
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference zoom;
    [SerializeField] InputActionReference rotateLeft;
    [SerializeField] InputActionReference rotateRight;

    Vector2 _moveDirection;
    Vector2 _zoomValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection = move.action.ReadValue<Vector2>();
        _zoomValue = zoom.action.ReadValue<Vector2>().normalized;

        float _zoomAmount = transform.position.z + (_zoomValue.y * zoomspeed);
        transform.position += new Vector3(_moveDirection.x * movespeed, _moveDirection.y * movespeed,
                                         _zoomAmount > minZoom ? minZoom - transform.position.z
                                         : (_zoomAmount < maxZoom ? maxZoom - transform.position.z : _zoomValue.y * zoomspeed));

        //TODO: Fix rotation to not mess up local 'up' direction
        // if (rotateLeft.action.IsPressed()) transform.Rotate(Vector3.back, rotatespeed);
        // if (rotateRight.action.IsPressed()) transform.Rotate(Vector3.back, -rotatespeed);
    }
}
