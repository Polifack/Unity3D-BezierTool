using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CurveCreator))]
public class CurveEditor : Editor
{
    CurveCreator creator;
    Curve curve;

    private void OnEnable()
    {
        // Cast the editor target as a path creator.
        // The target is the object that is being created
        creator = (CurveCreator)target;

        if (creator.curve == null)
        {
            creator.CreateCurve();
        }

        curve = creator.curve;
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    private void Draw()
    {
        // Draw Curve
        for (int i = 0; i< curve.NumSegments; i++)
        {
            Vector2[] segmentPoints = curve.GetPointsInSegment(i);

            // Draw lines between control points
            Handles.color = Color.black;
            Handles.DrawLine(segmentPoints[0], segmentPoints[1]);
            Handles.DrawLine(segmentPoints[2], segmentPoints[3]);

            // Draw the bezier line
            Handles.DrawBezier(segmentPoints[0], segmentPoints[3], segmentPoints[1], 
                segmentPoints[2], Color.green, null, 2);
        }

        
        // Draw Points
        Handles.color = Color.red;
        for (int i = 0; i < curve.NumPoints; i++)
        {
            Vector2 newPosition = Handles.FreeMoveHandle(curve[i], Quaternion.identity,.1f, 
                Vector2.zero, Handles.SphereHandleCap);

            // Check if we are moving the positions in the editor
            if (curve[i] != newPosition)
            {
                // Update the curve position and record an undo action for the editor
                Undo.RecordObject(creator, "Move point");
                curve.MovePoint(i, newPosition);
            }
        }
    }

    private void Input()
    {
        // Get the mouse position in the editor
        Event e = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;

        // Check if we are pressing the button while holding shift
        if (e.type==EventType.MouseDown && e.button==0 && e.shift)
        {
            // Add a segment to the curve in the position of the mouse and record it to be able to undo it
            Undo.RecordObject(creator, "Add point");
            curve.AddSegment(mousePos);
        }
    }

}
