using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Curve
{
    [SerializeField, HideInInspector]
    List<Vector2> points;
    [SerializeField, HideInInspector]
    bool isClosed;
    [SerializeField, HideInInspector]
    bool autoSetControlPoints;

    public Curve(Vector2 center)
    {
        //Creates a curve with four points with the center between them
        points = new List<Vector2>
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
            return points[i];
        }
    }

    public bool AutoSetControlPoints
    {
        get
        {
            return autoSetControlPoints;
        }
        set
        {
            if (autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                if (autoSetControlPoints)
                {
                    AutoSetAllControlPoints();
                }
            }
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            //Return the number of segments. We consider that the path may be open or closed.
            return (points.Count / 3);
        }
    }

    public void AddSegment(Vector2 newPoint)
    {
        // Add a new point to the curve

        Vector2 lastPoint = points[points.Count - 1];
        Vector2 secondLastPoint = points[points.Count - 2];

        // Add a new control point for the last point. This control point will be the oposite to the 
        // actual control point for the last point

        points.Add(lastPoint + (lastPoint - secondLastPoint));

        // Add the control point for the new point added. This control point will be in the half of the
        // current last point and the new point

        points.Add((lastPoint + newPoint) * 0.5f);

        // Add the new point
        points.Add(newPoint);

        if (autoSetControlPoints)
        {
            // Automatically compute controls
            AutoSetAffectedControlPoints(points.Count - 1);
        }
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        // Return the points that belong to a segment of the curve

        // The first point in a segment will be the third one, so...
        // Remark: We use loop index in the last curve point just in case we are in a closed curve
        int segment = i * 3;
        Vector2[] pointsInSegment = new Vector2[] { points[segment], points[segment+1],
            points[segment+2], points[LoopIndex(segment+3)]};

        return pointsInSegment;
    }

    public void MovePoint(int i, Vector2 newPosition)
    {
        Vector2 deltaMove = newPosition - points[i];
        points[i] = newPosition;

        if (autoSetControlPoints)
        {
            // If we are autosetting the control points we have to compute the affected ones
            AutoSetAffectedControlPoints(i);
            return;
        }

        // Check if we are moving a anchor point
        // anchor points has always an index that is modulus 3
        if (i % 3 == 0)
        {
            // If we move a anchor point, move his other control point too
            //
            if (i + 1 < points.Count || isClosed)
            {
                // Check if we have a "next" control point
                // the last point will not have a next control point
                points[LoopIndex(i + 1)] += deltaMove;
            }

            if (i - 1 >= 0 || isClosed)
            {
                // Check if we have a "previous" control point
                // the first point will not have a previous control point
                points[LoopIndex(i - 1)] += deltaMove;
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
            // or last point OR we are in a closed curve
            if (correspondingControlPointIndex >= 0 && correspondingControlPointIndex<points.Count || isClosed)
            {
                // Compute the distance and direction that we have to move the opposite control point
                float distance = (points[LoopIndex(anchorIndex)] - 
                    points[LoopIndex(correspondingControlPointIndex)]).magnitude;
                Vector2 direction = (points[LoopIndex(anchorIndex)] - newPosition).normalized;


                points[LoopIndex(correspondingControlPointIndex)] = points[LoopIndex(anchorIndex)] + 
                    direction * distance;
            }
        }
    }

    public void ToggleClosed()
    {
        isClosed = !isClosed;

        if (isClosed)
        {
            // Add a control point for the first point
            points.Add(points[points.Count - 1] + 
                (points[points.Count - 1] - points[points.Count - 2]));

            // Add an oposite point to the first point
            points.Add(points[0] * 2 - points[1]);

            if (autoSetControlPoints)
            {
                AutoSetAnchorControlPoints(0);
                AutoSetAnchorControlPoints(points.Count - 3);
            }
        }
        else
        {
            //Remove the oposite point to the first and the control point for the last
            points.RemoveRange(points.Count - 2,2);

            if (autoSetControlPoints)
            {
                AutoSetFirstLastControlPoints();
            }
        }
    }

    private void AutoSetAffectedControlPoints(int anchor)
    {
        for (int i = anchor - 3; i < anchor + 3; i += 3)
        {
            if (i >= 0 && i <= points.Count || isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }

        AutoSetFirstLastControlPoints();
    }

    private void AutoSetAllControlPoints()
    {
        // Set the point for the anchors in the middle of the curve
        for (int i = 0; i < points.Count; i+=3) 
        {
            AutoSetAnchorControlPoints(i);
        }

        // Set the first and last
        AutoSetFirstLastControlPoints();
    }

    private void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPos = points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0 || isClosed)
        {
            // Check if we are not in the first anchor point
            Vector2 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }

        if (anchorIndex + 3 >= 0 || isClosed)
        {
            // Check if we are in the last anchor point
            Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }


        dir=dir.normalized;

        for (int i = 0; i < 2; i++)
        { 
            // Set the automatically calculated control points

            int controlPointIndex = anchorIndex + i * 2 - 1;
            if (controlPointIndex >= 0 && controlPointIndex < points.Count || isClosed)
            {
                points[LoopIndex(controlPointIndex)] = anchorPos + dir * neighbourDistances[i] * 0.5f;
            }
        }
    }

    private void AutoSetFirstLastControlPoints()
    {
        // For a first and last control point we need that the curve is not closed
        if (!isClosed)
        {
            points[1] = (points[0] + points[2]) * 0.5f;
            points[points.Count-2] = (points[points.Count-1] + points[points.Count-3]) * 0.5f;
        }
    }

    private int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
}
