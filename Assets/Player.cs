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

    private Matrix4x4 _containersWorldMatrix;

    private SplinePath _path;
    private float _distanceTravelledAlongPath;
    private float _yVelocity;
    private float _movementDirection;
    private float _distanceToTravel;

    private bool _isGrounded = true;

    private void Start()
    {
        _containersWorldMatrix = splineContainer.transform.localToWorldMatrix;

        _path = CreateSplinePath(splineContainer, _containersWorldMatrix);
    }

    private void Update()
    {
        if (!_isGrounded)
        {
            _yVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;
            transform.Translate(new Vector3(0f, _yVelocity, 0f) * Time.deltaTime);

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
        if (_yVelocity > 0)
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
        _yVelocity = Mathf.Sqrt(jumpHeight * -2 * (Physics.gravity.y * gravityScale));
        transform.Translate(new Vector3(0f, _yVelocity, 0f) * Time.deltaTime);
        _isGrounded = false;
    }

    private void CalculateDistanceTravelled()
    {
        _distanceToTravel = _movementDirection * speed * Time.deltaTime;
        _distanceTravelledAlongPath += _distanceToTravel;
        _distanceTravelledAlongPath = Mathf.Clamp01(_distanceTravelledAlongPath);
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
        return _path.EvaluatePosition(_distanceTravelledAlongPath);
    }

    private Vector3 GetPathRotation()
    {
        return _path.EvaluateTangent(_distanceTravelledAlongPath);
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