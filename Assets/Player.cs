using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Player : MonoBehaviour
{
    [SerializeField] private Animator charAnimator;

    [SerializeField] private SplineContainer splineContainer;

    [SerializeField] private float speed = 0.01f;
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float gravityScale = 5f;

    private Matrix4x4 containersWorldMatrix;

    private SplinePath path;
    private float distanceTravelledAlongPath;
    private float yVelocity;
    private float _movementDirection;
    private float _distanceToTravel;

    private bool _isGrounded = true;

    // Start is called before the first frame update
    private void Start()
    {
        containersWorldMatrix = splineContainer.transform.localToWorldMatrix;

        path = CreateSplinePath(splineContainer, containersWorldMatrix);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_isGrounded)
        {
            yVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;
            transform.Translate(new Vector3(0f, yVelocity, 0f) * Time.deltaTime);

            CheckForCollisionWithGround();
        }

        HandleInput();

        if (_movementDirection == 0f)
            return;

        CalculateDistanceTravelled();

        Move();
        Rotate();
    }

    private void CheckForCollisionWithGround()
    {
        if (yVelocity > 0)
            return;

        //Debug.DrawRay(transform.position, Vector3.down, Color.black, 10f);
        //Debug.DrawLine(transform.position, transform.position - Vector3.down * 0.5f, Color.red, 10f);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.5f))
        {
            //Debug.Log(hit.point);
            //Debug.DrawLine(transform.position, hit.point, Color.blue, 10f);
            //Debug.Log(transform.position.y);
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            //Debug.Log(transform.position.y);
            _isGrounded = true;
        }
    }

    private void HandleInput()
    {
        _movementDirection = Input.GetAxis("Horizontal");

        charAnimator.SetFloat("Speed", Mathf.Abs(_movementDirection));

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        yVelocity = Mathf.Sqrt(jumpHeight * -2 * (Physics.gravity.y * gravityScale));
        transform.Translate(new Vector3(0f, yVelocity, 0f) * Time.deltaTime);
        _isGrounded = false;
    }

    private void CalculateDistanceTravelled()
    {
        _distanceToTravel = _movementDirection * speed * Time.deltaTime;
        distanceTravelledAlongPath += _distanceToTravel;
        distanceTravelledAlongPath = Mathf.Clamp01(distanceTravelledAlongPath);
    }

    private void Move()
    {
        Vector3 newPosition = GetPathPosition();

        if (!_isGrounded)
        {
            newPosition = new Vector3(newPosition.x, transform.position.y, newPosition.z);
        }

        transform.position = newPosition;

    }

    private void Rotate()
    {
        Vector3 newRotation = GetPathRotation() * _movementDirection;

        if (!_isGrounded)
        {
            newRotation = new Vector3(newRotation.x, 0f, newRotation.z);
        }

        transform.LookAt(transform.position + newRotation);
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
            SplineSlice<Spline> slice = CreateSplineSlice(spline, 0, 1, containersWorldMatrix);
            splineSlices.Add(slice);
        }

        return new SplinePath(splineSlices.ToArray());
    }

    private SplineSlice<Spline> CreateSplineSlice(Spline spline, int startKnot, int direction, Matrix4x4 containersWorldMatrix)
    {
        return new SplineSlice<Spline>(spline, new SplineRange(startKnot, spline.Count * direction), containersWorldMatrix);
    }
}