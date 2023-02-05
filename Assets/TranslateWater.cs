using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateWater : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0f, 0.5f, 0f) * Time.deltaTime);   
    }
}
