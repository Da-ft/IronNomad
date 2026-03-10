using UnityEngine;

public class ConveyorCurve : ConveyorBelt
{
    [Header("Curve")]
    [SerializeField] private Transform _controlPoint;

    private const int CurveSamples = 20;

    /// <summary>
    /// Überschreibt InitializeEndpoints aus ConveyorBelt.
    /// Berechnet den Bezier-Kontrollpunkt automatisch aus Start und End.
    /// </summary>
    public new void InitializeEndpoints(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        base.InitializeEndpoints(startWorldPos, endWorldPos);
        SetAutoControlPoint(startWorldPos, endWorldPos);
    }

    /// <summary>
    /// Kontrollpunkt = Ecke der L-Form zwischen Start und End.
    /// Das ist der Punkt der die Kurve "aufspannt".
    /// </summary>
    private void SetAutoControlPoint(Vector3 start, Vector3 end)
    {
        if (_controlPoint == null)
        {
            _controlPoint = new GameObject("ControlPoint").transform;
            _controlPoint.SetParent(transform);
        }

        // Ecke: gleiche X wie End, gleiche Z wie Start
        _controlPoint.position = new Vector3(end.x, start.y, start.z);
    }

    protected override void CalculateSpeed()
    {
        if (_controlPoint == null) return;
        _beltLength = CalculateCurveLength();
        _speed = _beltLength / (60f / _itemsPerMinute);
    }

    protected override Vector3 GetPositionOnPath(float t)
    {
        return BezierPoint(t);
    }

    private Vector3 BezierPoint(float t)
    {
        if (_controlPoint == null)
            return Vector3.Lerp(_startPoint.position, _endPoint.position, t);

        float u = 1f - t;
        return (u * u * _startPoint.position)
             + (2f * u * t * _controlPoint.position)
             + (t * t * _endPoint.position);
    }

    private float CalculateCurveLength()
    {
        float length = 0f;
        Vector3 prev = BezierPoint(0f);

        for (int i = 1; i <= CurveSamples; i++)
        {
            Vector3 curr = BezierPoint(i / (float)CurveSamples);
            length += Vector3.Distance(prev, curr);
            prev = curr;
        }

        return length;
    }
}
