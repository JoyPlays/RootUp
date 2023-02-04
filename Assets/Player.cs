using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Player : MonoBehaviour
{
    [SerializeField] private Animator charAnimator;

    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Vector3 offsetFromPath;

    [SerializeField] private float speed = 0.01f;
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float gravityScale = 5f;

    private SplinePath path;
    private float distanceTravelledAlongPath;
    private float yVelocity;
    private MovementDirection travelDirection;

    // Start is called before the first frame update
    private void Start()
    {
        Matrix4x4 containersWorldMatrix = splineContainer.transform.localToWorldMatrix;

        path = CreateSplinePath(splineContainer, containersWorldMatrix);
    }

    // Update is called once per frame
    private void Update()
    {
        yVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;

        HandleInput();

        MoveAndRotate();

        //transform.Translate(new Vector3(0f, yVelocity, 0f) * Time.deltaTime);
    }

    private void HandleInput()
    {
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
    }

    private void MoveAndRotate()
    {
        if (travelDirection == MovementDirection.NOT_MOVING)
            return;

        float distanceToTravel = speed * Time.fixedDeltaTime;

        distanceToTravel *= (int)travelDirection;
        distanceTravelledAlongPath += distanceToTravel;

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