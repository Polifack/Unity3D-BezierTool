# Unity 3D Bezier Tool

Tool that allows to create bezier curves in 3D space in Unity.


## Project structure


### Curve.cs
Manages the logic of the bezier curve. Allows for keep the position of points, add points move points and remove points.

### CurveCreator.cs
Manages the creation of the curve. Is the component that will be used in the unity editor.

### CurveEditor.cs
Manages the interaction between the tool and unity editor. Allows for undo/redo, point-and-click addition of points and drag move points.


## Bezier curves

In order to create a bezier curve we will use the Linear Interpolation, the Quadratic Curves and the Cubic Curves.

### Linear Interpolation

The Linear Interpolation or Lerp between two points in a time t will return the position of an object that is moving between a and b in time t.

```Vector2 Lerp(Vector2 a, Vector2 b, float time)```

### Quadratic Curves

The Quadratic curve formed by 3 points a, b and c will return the curve formed between the points that Linealy Interpolate between a and b and b and c.

```Vector2 QuadraticCurve(Vector2 a, Vector2 b, Vector2 c, float time)```

### Cubic Curves

The Cubic curve formed by 4 points a, b, c and d will return the curve formed between the points that forme a Quadratic Curve between the a, b and c and b, c and d.

```Vector2 CubicCurve(Vector2 a, Vector2 b, Vector2 c, float time)```


## Funcionalities

### Add and Remove points in scene
Add points to a curve in the unity editor using shift+click

### Auto close curve
Bezier curve can be auto-closed by toggling a editor button

### Auto compute control points
Toggle funcionality that allows unity to compute the anchor point by the half point of the distance between each anchor