using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 5.0f;
    public float rotateSpeed = 180f;
    public float jumpHeight = 1.5f;
    public float gravityValue = -9.81f;

    public CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

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
        Debug.Log(playerRotate);
        gameObject.transform.Rotate(Vector3.up, playerRotate * rotateSpeed * Time.deltaTime);

        

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
