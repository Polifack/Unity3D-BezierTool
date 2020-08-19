using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveCreator : MonoBehaviour
{
    [HideInInspector]
    public Curve curve;

    public void CreateCurve()
    {
        curve = new Curve(transform.position);
    }

}
