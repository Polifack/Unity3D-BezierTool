using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Curve
{
    [SerializeField, HideInInspector]
    List<Vector2> curvePoints;

    public Curve(Vector2 center)
    {
        //Creates a curve with four points with the center between them
        curvePoints = new List<Vector2>
        {
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * 0.5f,
            center + Vector2.right,
            center + (Vector2.right - Vector2.up) * 0.5f
        };
    }

    public Vector2 this[int i]
    {
        //make the Curve class able to return something if we do curve[i]
        get
        {
            return curvePoints[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return curvePoints.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            //Return the number of segments. The first segment has 4 points and the rest has 3.
            return (curvePoints.Count - 4) / 3 + 1;
        }
    }

    public void AddSegment(Vector2 newPoint)
    {
        // Add a new point to the curve

        Vector2 lastPoint = curvePoints[curvePoints.Count - 1];
        Vector2 secondLastPoint = curvePoints[curvePoints.Count - 2];

        // Add a new control point for the last point. This control point will be the oposite to the 
        // actual control point for the last point

        curvePoints.Add(lastPoint + (lastPoint - secondLastPoint));

        // Add the control point for the new point added. This control point will be in the half of the
        // current last point and the new point

        curvePoints.Add((lastPoint + newPoint) * 0.5f);

        // Add the new point

        curvePoints.Add(newPoint);
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        // Return the points that belong to a segment of the curve

        // The first point in a segment will be the third one, so
        int segment = i * 3;
        Vector2[] pointsInSegment = new Vector2[] { curvePoints[segment], curvePoints[segment+1],
            curvePoints[segment+2], curvePoints[segment+3]};

        return pointsInSegment;
    }

    public void MovePoint(int i, Vector2 newPosition)
    {
        Vector2 deltaMove = newPosition - curvePoints[i];
        curvePoints[i] = newPosition;

        // Check if we are moving a anchor point
        // anchor points has always an index that is modulus 3
        if (i % 3 == 0)
        {
            // If we move a anchor point, move his other control point too
            if (i + 1 < curvePoints.Count)
            {
                // Check if we have a "next" control point
                // the last point will not have a next control point
                curvePoints[i + 1] += deltaMove;
            }

            if (i - 1 >= 0)
            {
                // Check if we have a "previous" control point
                // the first point will not have a previous control point
                curvePoints[i - 1] += deltaMove;
            }
        }

        // Check if we are moving a control point
        else
        {
            // If the next point is anchor, then the other control point will be 2 points ahead
            bool nextPointIsAnAnchorPoint = (i + 1 % 3) == 0;

            // Get the other control point index
            int correspondingControlPointIndex = nextPointIsAnAnchorPoint ? i + 2 : i - 2;
            
            // Get the anchor index
            int anchorIndex = nextPointIsAnAnchorPoint ? i + 1 : i - 1;

            // Check that the index of the control point exists, that means, we are not the first
            // or last point
            if (correspondingControlPointIndex >= 0 && correspondingControlPointIndex<curvePoints.Count)
            {
                // Compute the distance and direction that we have to move the opposite control point
                float distance = (curvePoints[anchorIndex] - curvePoints[correspondingControlPointIndex]).magnitude;
                Vector2 direction = (curvePoints[anchorIndex] - newPosition).normalized;


                curvePoints[correspondingControlPointIndex] = curvePoints[anchorIndex] + direction * distance;
            }
        }
    }


}
