using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera UICamera;    // Assign main camera
    public Camera topDownCamera; // Assign top-down camera


    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    public bool isTopDown = true;

    void Start()
    {
        UICamera.enabled = false;
        topDownCamera.enabled = true;
    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Tab)) // Return is the "Enter" key
        {
            // Toggle between cameras
            isTopDown = !isTopDown;
            UICamera.enabled = !isTopDown;
            topDownCamera.enabled = isTopDown;
        }
        */

        // If we're in top-down camera mode, enable WASD movement and zooming
        if (topDownCamera.enabled)
        {
            HandleTopDownCameraControls();
        }
    }

    void HandleTopDownCameraControls()
    {
        // WASD movement
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime; // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;   // W/S or Up/Down

        // Move the top-down camera
        topDownCamera.transform.position += new Vector3(moveX, 0, moveZ);

        // Mouse wheel zooming
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            // Adjust the camera's height (zoom in and out)
            float newHeight = topDownCamera.transform.position.y - scrollInput * zoomSpeed;
            newHeight = Mathf.Clamp(newHeight, minZoom, maxZoom); // Clamp between min and max zoom

            topDownCamera.transform.position = new Vector3(
                topDownCamera.transform.position.x, 
                newHeight, 
                topDownCamera.transform.position.z
            );
        }
    }
}