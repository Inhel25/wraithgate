using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Gerakan player top-down 2D untuk WRAITHGATE (Unity 6).
/// W = Back (atas), S = Front (bawah), A = Left, D = Right.
/// Memakai Animator: FacingX (float), FacingY (float), IsMoving (bool).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("MOVEMENT")]
    [Min(0f)]
    [SerializeField] private float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    // Saat pertama muncul di Village, menghadap Back (atas layar).
    private Vector2 facingDirection = Vector2.up;

    private static readonly int FacingX = Animator.StringToHash("FacingX");
    private static readonly int FacingY = Animator.StringToHash("FacingY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Arah hadap langsung disiapkan ketika prefab dibuat oleh spawner.
        moveInput = Vector2.zero;
        facingDirection = Vector2.up;
        ApplyAnimatorParameters(false);
    }

    private void Start()
    {
        // Paksa keadaan awal saat player di-spawn setelah Character Selection.
        moveInput = Vector2.zero;
        facingDirection = Vector2.up;
        ApplyAnimatorParameters(false);
    }

    private void Update()
    {
        Vector2 rawInput = ReadKeyboardInput();
        bool isMoving = rawInput.sqrMagnitude > 0f;

        // Normalisasi agar bergerak diagonal tidak lebih cepat.
        moveInput = isMoving ? rawInput.normalized : Vector2.zero;

        if (isMoving)
        {
            // Pilih satu arah cardinal untuk animasi 4 arah.
            // Jika W+D ditekan bersamaan, prioritas arah vertikal.
            if (Mathf.Abs(rawInput.y) >= Mathf.Abs(rawInput.x))
            {
                facingDirection = rawInput.y > 0f
                    ? Vector2.up   // W = Back
                    : Vector2.down; // S = Front
            }
            else
            {
                facingDirection = rawInput.x > 0f
                    ? Vector2.right // D = Right
                    : Vector2.left; // A = Left
            }
        }

        // Saat tidak bergerak, jangan reset facingDirection.
        // Jadi Idle mengikuti arah terakhir.
        ApplyAnimatorParameters(isMoving);
    }

    private void FixedUpdate()
    {
        // Untuk Rigidbody2D Dynamic, gerakkan lewat physics.
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void ApplyAnimatorParameters(bool isMoving)
    {
        if (animator == null)
            return;

        animator.SetFloat(FacingX, facingDirection.x);
        animator.SetFloat(FacingY, facingDirection.y);
        animator.SetBool(IsMoving, isMoving);
    }

    private Vector2 ReadKeyboardInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

#if ENABLE_INPUT_SYSTEM
        // Berjalan juga ketika Active Input Handling = Input System / Both.
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return Vector2.zero;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            horizontal += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            vertical -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            vertical += 1f;
#else
        // Berjalan ketika Active Input Handling = Input Manager (Old).
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical += 1f;
#endif

        return new Vector2(horizontal, vertical);
    }
}
