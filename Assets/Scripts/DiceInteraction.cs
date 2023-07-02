using UnityEngine;
using UnityEngine.UIElements;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

public class DiceInteraction : MonoBehaviour
{
    private Rigidbody diceRigidbody;
    private Collider diceCollider;
    private bool isGrabbed = false;
    private Vector3 fingerOffset;
    private Vector2 initialFingerPosition;
    private Vector2 accumulatedSwipeDelta;

    Vector3 accelerationDir; // Phone shake
    Collision collision;

    private void Start()
    {
        diceRigidbody = GetComponent<Rigidbody>();
        diceCollider = GetComponent<Collider>();
        diceRigidbody.useGravity = false; // Disable gravity initially
    }

    private void Update()
    {
        // Detect phone shake
        accelerationDir = Input.acceleration;

        if (!isGrabbed && accelerationDir.sqrMagnitude >= 5f)
        {
            RumbleDice(accelerationDir.sqrMagnitude);
        }

        // Remove dice if it's fallen into the abyss
        if (transform.position.y < -100)
        {
            Destroy(gameObject);
        }
    }

    private void RumbleDice(float magnitude)
    {
        // Limit max magnitude to 1.0
        if (magnitude > 1.0f) 
        {
            magnitude = 1.0f;
        }

        // Magnitude of the random sideways force
        float sidewaysForceMagnitude = 0.03f;

        // Magnitude of the random rotational force
        float rotationalForceMagnitude = 50f;

        // Apply the rumble force to the dice when the phone is shaken,
        // if the die is colliding with something
        if (collision != null)
        {
            diceRigidbody.AddForce(Vector3.up * magnitude, ForceMode.Impulse);
            diceRigidbody.AddForce(Random.insideUnitCircle.normalized * sidewaysForceMagnitude, ForceMode.Impulse);
            diceRigidbody.AddTorque(Random.insideUnitSphere * rotationalForceMagnitude, ForceMode.Impulse);
        }
        
    }

    void OnCollisionEnter(Collision collision)
    {
        this.collision = collision;
    }

    void OnCollisionExit(Collision collision)
    {
        this.collision = null;
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
        EnhancedTouch.Touch.onFingerMove += FingerMove;
        EnhancedTouch.Touch.onFingerUp += FingerUp;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
        EnhancedTouch.Touch.onFingerMove -= FingerMove;
        EnhancedTouch.Touch.onFingerUp -= FingerUp;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        Ray raycast = Camera.main.ScreenPointToRay(finger.screenPosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(raycast, out raycastHit))
        {
            if (raycastHit.collider == diceCollider)
            {
                isGrabbed = true;
                fingerOffset = transform.position - raycastHit.point;
                diceRigidbody.useGravity = false;

                // Store the initial finger position for calculating swipe delta
                initialFingerPosition = finger.screenPosition;
                accumulatedSwipeDelta = Vector2.zero;
            }
        }
    }

    private void FingerMove(EnhancedTouch.Finger finger)
    {
        if (isGrabbed)
        {
            // Move the dice on the x and y axis
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(finger.screenPosition.x, finger.screenPosition.y, 0.2f));
            transform.position = targetPosition;

            // Apply rotational force to the dice based on finger movement
            Vector2 swipeDelta = finger.screenPosition - initialFingerPosition;
            float torqueForce = 0.00002f; // Adjust the torque force as needed
            Vector3 torque = new Vector3(-swipeDelta.y, swipeDelta.x, 0f) * torqueForce;
            diceRigidbody.AddTorque(torque);

            // Accumulate the swipe delta
            accumulatedSwipeDelta += finger.screenPosition - initialFingerPosition;
            initialFingerPosition = finger.screenPosition;
        }
    }

    private void FingerUp(EnhancedTouch.Finger finger)
    {
        if (isGrabbed)
        {
            isGrabbed = false;
            diceRigidbody.useGravity = true;
            diceRigidbody.velocity = CalculateThrowVelocity(accumulatedSwipeDelta);
            accumulatedSwipeDelta = Vector2.zero;
        }
    }

    private Vector3 CalculateThrowVelocity(Vector2 swipeDelta)
    {
        // Adjust the following values based on the desired throwing behavior
        float throwForce = 2f; // Controls the overall force of the throw
        float maxSwipeMagnitude = 1000f; // Controls the maximum swipe length
        float maxThrowForce = 10f; // Controls the maximum force applied

        // Calculate the normalized swipe delta and adjust for the maximum swipe magnitude
        float normalizedSwipeMagnitude = Mathf.Min(swipeDelta.magnitude, maxSwipeMagnitude) / maxSwipeMagnitude;
        Vector2 normalizedSwipeDirection = swipeDelta.normalized;

        // Calculate the throw velocity based on the swipe direction and force
        float throwForceMultiplier = throwForce * normalizedSwipeMagnitude;
        float throwForceClamped = Mathf.Clamp(throwForceMultiplier, 0f, maxThrowForce);
        Vector3 throwVelocity = new Vector3(normalizedSwipeDirection.x, 0f, normalizedSwipeDirection.y) * throwForceClamped;

        return throwVelocity;
    }
}