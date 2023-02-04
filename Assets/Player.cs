using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Player : MonoBehaviour
{
    [SerializeField] private Animator charAnimator;

    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Vector3 offsetFromPath;

    [SerializeField] private float speed = 0.01f;
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float gravityScale = 5f;

    private Matrix4x4 containersWorldMatrix;

    private SplinePath path;
    private float distanceTravelledAlongPath;
    private float yVelocity;
    private MovementDirection travelDirection;

    private bool isGrounded = true;

    // Start is called before the first frame update
    private void Start()
    {
        containersWorldMatrix = splineContainer.transform.localToWorldMatrix;

        path = CreateSplinePath(splineContainer, containersWorldMatrix);
    }

    // Update is called once per frame
    private void Update()
    {
        yVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;

        HandleInput();

        if (isGrounded)
        {
            MoveAndRotate();
        }
        else
        {
            NativeSpline native = new NativeSpline(path, containersWorldMatrix);
            float distance = SplineUtility.GetNearestPoint(native, transform.position, out float3 position, out float t);
            Debug.Log(distance);
            if (distance < 0.5f)
            {
                distanceTravelledAlongPath = t;
                transform.position = GetPathPosition();
                isGrounded = true;
                return;
            }

            transform.Translate(new Vector3(0f, yVelocity, 0f) * Time.deltaTime);
        }
    }

    private void HandleInput()
    {
        if (!isGrounded)
            return;

        travelDirection = MovementDirection.NOT_MOVING;
        charAnimator.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Horizontal")));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.D))
        {
            travelDirection = MovementDirection.FORWARD;
        }

        if (Input.GetKey(KeyCode.A))
        {
            travelDirection = MovementDirection.BACKWARD;
        }
    }

    private void Jump()
    {
        yVelocity = Mathf.Sqrt(jumpHeight * -2 * (Physics.gravity.y * gravityScale));
        transform.Translate(new Vector3(0f, yVelocity, 0f) * Time.deltaTime);
        isGrounded = false;
    }

    private void MoveAndRotate()
    {
        if (travelDirection == MovementDirection.NOT_MOVING)
            return;

        float distanceToTravel = speed * Time.fixedDeltaTime;

        distanceToTravel *= (int)travelDirection;
        distanceTravelledAlongPath += distanceToTravel;
        distanceTravelledAlongPath = Mathf.Clamp01(distanceTravelledAlongPath);

        Vector3 newPosition = GetPathPosition() + offsetFromPath;
        Vector3 newRotation = newPosition + (GetPathRotation() * (int)travelDirection);

        transform.position = newPosition;
        transform.LookAt(newRotation);
    }

    private Vector3 GetPathPosition()
    {
        return path.EvaluatePosition(distanceTravelledAlongPath);
    }

    private Vector3 GetPathRotation()
    {
        return path.EvaluateTangent(distanceTravelledAlongPath);
    }

    private SplinePath CreateSplinePath(SplineContainer splineContainer, Matrix4x4 containersWorldMatrix)
    {
        List<SplineSlice<Spline>> splineSlices = new();

        foreach (Spline spline in splineContainer.Splines)
        {
            SplineSlice<Spline> slice = CreateSplineSlice(spline, 0, (int)MovementDirection.FORWARD, containersWorldMatrix);
            splineSlices.Add(slice);
        }

        return new SplinePath(splineSlices.ToArray());
    }

    private SplineSlice<Spline> CreateSplineSlice(Spline spline, int startKnot, int direction, Matrix4x4 containersWorldMatrix)
    {
        return new SplineSlice<Spline>(spline, new SplineRange(startKnot, spline.Count * direction), containersWorldMatrix);
    }
}

public enum MovementDirection
{
    FORWARD = 1,
    BACKWARD = -1,
    NOT_MOVING = 0
}