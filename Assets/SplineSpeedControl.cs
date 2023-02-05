using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineSpeedControl : MonoBehaviour
{
    [SerializeField] private GameObject _destroyedAcorn;
    [SerializeField] private SplineAnimate _splineAnimate;

    private void Start() {
        _splineAnimate = GetComponent<SplineAnimate>();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Destroy")
        {
            GameObject acorn = Instantiate(_destroyedAcorn, transform.position, transform.rotation);
            _splineAnimate.Restart(true);
            Destroy(acorn, 3f);
            
        }
    }
}
