using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ruliprueba : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 3f;
	[SerializeField] private float maxFallSpeed = 10f;

	private Rigidbody2D rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.gravityScale = Mathf.Max(0f, rb.gravityScale);
	}

	private float inputX;

	private void Update()
	{
		inputX = 0f;

		Keyboard keyboard = Keyboard.current;
		if (keyboard == null)
		{
			return;
		}

		if (keyboard.rightArrowKey.isPressed)
		{
			inputX = 1f;
		}
		else if (keyboard.leftArrowKey.isPressed)
		{
			inputX = -1f;
		}
	}

	private void FixedUpdate()
	{
		MoveHorizontal(inputX);
	}

	private void MoveHorizontal(float inputX)
	{
		Vector2 velocity = rb.linearVelocity;
		velocity.x = inputX * moveSpeed;
		velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
		rb.linearVelocity = velocity;
	}
}
