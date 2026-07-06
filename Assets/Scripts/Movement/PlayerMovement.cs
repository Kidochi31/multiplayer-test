using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 5.0f;
    public float rotateSpeed = 180f;
    public float cameraUpDownSpeed = 180f;
    public float jumpHeight = 1.5f;
    public float gravityValue = -9.81f;
    public float minYAngle = -60f;
    public float maxYAngle = 60f;
    public GameObject Camera;

    public CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    public float rotationX = 0f;

    void Update()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer)
        {
            // Slight downward velocity to keep grounded stable
            if (playerVelocity.y < -2f)
                playerVelocity.y = -2f;
        }
        // rotate
        float playerRotate = KeyInput.GetStickBinding<CameraStick>().Value.x;
        gameObject.transform.Rotate(Vector3.up, playerRotate * rotateSpeed * Time.deltaTime);

        float cameraY = KeyInput.GetStickBinding<CameraStick>().Value.y;
        rotationX -= cameraY * cameraUpDownSpeed * Time.deltaTime;

        // Clamp the rotation so the camera doesn't flip over
        rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

        // Apply only the X rotation while preserving current Y and Z rotations
        Camera.transform.rotation = Quaternion.Euler(rotationX, Camera.transform.rotation.eulerAngles.y, 0f);

        

        // Jump using WasPressedThisFrame()

        if (groundedPlayer && KeyInput.GetKeyBinding<JumpKey>().DownThisFrame)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }

        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;

        // Move
        Vector2 input = KeyInput.GetStickBinding<MoveStick>().Value;
        Vector3 move = transform.forward * input.y + transform.right * input.x;
        Vector3 finalMove = move * playerSpeed + Vector3.up * playerVelocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }
}
