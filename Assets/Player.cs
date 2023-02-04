using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Player : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Vector3 offsetFromPath;
    [SerializeField] private float speed = 0.05f;

    private SplinePath path;
    private float distanceTravelledAlongPath;

    private const int FORWARD_MOVEMENT = 1;
    private const int BACKWARD_MOVEMENT = -1;

    // Start is called before the first frame update
    private void Start()
    {
        Matrix4x4 containersWorldMatrix = splineContainer.transform.localToWorldMatrix;

        path = CreateSplinePath(splineContainer, containersWorldMatrix);
    }

    // Update is called once per frame
    private void Update()
    {
        float distanceToTravel = speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.D))
        {
            distanceTravelledAlongPath += distanceToTravel;
        }

        if (Input.GetKey(KeyCode.A))
        {
            distanceTravelledAlongPath -= distanceToTravel;
        }

        Vector3 pos = path.EvaluatePosition(distanceTravelledAlongPath);
        transform.position = pos + offsetFromPath;
    }

    private SplinePath CreateSplinePath(SplineContainer splineContainer, Matrix4x4 containersWorldMatrix)
    {
        List<SplineSlice<Spline>> splineSlices = new();

        foreach (Spline spline in splineContainer.Splines)
        {
            SplineSlice<Spline> slice = CreateSplineSlice(spline, 0, FORWARD_MOVEMENT, containersWorldMatrix);
            splineSlices.Add(slice);
        }

        return new SplinePath(splineSlices.ToArray());
    }

    private SplineSlice<Spline> CreateSplineSlice(Spline spline, int startKnot, int direction, Matrix4x4 containersWorldMatrix)
    {
        return new SplineSlice<Spline>(spline, new SplineRange(startKnot, spline.Count * direction), containersWorldMatrix);
    }
}
