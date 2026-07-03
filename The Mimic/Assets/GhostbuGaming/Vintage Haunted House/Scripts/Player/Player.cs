using UnityEngine;

namespace GhostbuGaming.VintageHauntedHouse
{

    public class Player : MonoBehaviour
    {
        public new Transform camera;
        public GameInput gameInput;

        private Vector3 moveDirection;
        private float speed = 5f;
        private float xRotation = 0;
        private Vector2 direction, look;

        [SerializeField] private CharacterController character;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float minimumXRotation = -90f;
        [SerializeField] private float maximumXRoatation = 90f;
        [SerializeField] private float cameraLerpMultiplier = 1f;
        private void OnEnable()
        {
            gameInput.Player.Enable();
        }

        private void Awake()
        {
            gameInput = new GameInput();
            speed = walkSpeed;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void FixedUpdate()
        {
            Movement();
            Gravity();
            Camera();
        }

        private void Gravity()
        {
            if (!Physics.CheckSphere(transform.position, 0.25f, groundMask))
            {
                moveDirection.y += Physics.gravity.y * Time.deltaTime;
            }
        }

        private void Movement()
        {
            direction = gameInput.Player.Move.ReadValue<Vector2>().normalized;
            moveDirection = transform.forward * direction.y + transform.right * direction.x + transform.up * moveDirection.y;
            character.Move(moveDirection * speed * Time.deltaTime);
            gameInput.Player.Sprint.performed += ctx =>
            {
                speed = runSpeed;
            };
            gameInput.Player.Sprint.canceled += ctx =>
            {
                speed = walkSpeed;
            };
        }

        private void Camera()
        {
            look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity * Time.deltaTime;
            xRotation -= look.y;
            xRotation = Mathf.Clamp(xRotation, -minimumXRotation, maximumXRoatation);
            transform.Rotate(0, look.x, 0);
            camera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        private void OnDisable()
        {
            gameInput.Player.Disable();
        }
    }
}