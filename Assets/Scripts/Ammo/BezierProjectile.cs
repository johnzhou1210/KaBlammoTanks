using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BezierProjectile : NetworkBehaviour {
    private const int SampleResolution = 200;
    [Header("Path Settings")] public Vector3 start;
    public Vector3 end;
    public float arcHeight = 5f;
    public float duration = 1f;
    private readonly List<float> _arcLengthTable = new(); // normalized distance [0-1] → t
    
    private NetworkVariable<bool> _moving = new NetworkVariable<bool>();
    private NetworkVariable<Vector3> _p1 = new NetworkVariable<Vector3>(); 
    private NetworkVariable<Vector3> _p2 = new NetworkVariable<Vector3>();
    private NetworkVariable<float> _speed = new NetworkVariable<float>();
    private NetworkVariable<float> _totalLength = new NetworkVariable<float>();
    private NetworkVariable<float> _distanceTraveled = new NetworkVariable<float>();

    private void Update() {
        if (!IsServer) return;
        if (!_moving.Value)
            return;
        _distanceTraveled.Value += Time.deltaTime * _speed.Value;
        if (_distanceTraveled.Value >= _totalLength.Value) {
            transform.position = end;
            _moving.Value = false;
            GetComponent<AmmoCollision>().Collide(true, true);
            return;
        }

        transform.position = GetCurrentPosition();
    }

    public void Launch(Vector3 startPos, Vector3 endPos, float arcH, float dur) {
        if (!IsServer) return;
        start = startPos;
        end = endPos;
        arcHeight = arcH;
        duration = dur;
        _p1.Value = start + Vector3.up * arcHeight;
        _p2.Value = end + Vector3.up * arcHeight;
        BuildArcLengthTable();
        _speed.Value = _totalLength.Value / duration;
        _distanceTraveled.Value = 0f;
        _moving.Value = true;
    }

    private Vector3 GetPoint(float t) {
        return Mathf.Pow(1 - t, 3) * start + 3 * Mathf.Pow(1 - t, 2) * t * _p1.Value + 3 * (1 - t) * Mathf.Pow(t, 2) * _p2.Value +
               Mathf.Pow(t, 3) * end;
    }

    private void BuildArcLengthTable() {
        _arcLengthTable.Clear();
        float length = 0f;
        _arcLengthTable.Add(0f);
        Vector3 prev = GetPoint(0f);
        for (int i = 1; i <= SampleResolution; i++) {
            float t = i / (float)SampleResolution;
            Vector3 point = GetPoint(t);
            length += Vector3.Distance(prev, point);
            prev = point;
            _arcLengthTable.Add(length);
        }

        _totalLength.Value = length;

        // Normalize the table to 0–1 for easier lookup
        for (int i = 0; i < _arcLengthTable.Count; i++) _arcLengthTable[i] /= _totalLength.Value;
    }

    private float GetTFromArcLength(float normalizedDistance) {
        for (int i = 0; i < _arcLengthTable.Count - 1; i++) {
            float d0 = _arcLengthTable[i];
            float d1 = _arcLengthTable[i + 1];
            if (normalizedDistance >= d0 && normalizedDistance <= d1) {
                float segmentT = (float)i / SampleResolution;
                float segmentLength = d1 - d0;
                float withinSegment = (normalizedDistance - d0) / segmentLength;
                return segmentT + withinSegment * (1f / SampleResolution);
            }
        }

        return 1f; // fallback
    }

    [ClientRpc]
    public void FlipXClientRpc() {
        transform.Rotate(0f, 180f, 0f);
        TextMeshPro tmPro = transform.GetComponentInChildren<TextMeshPro>();
        if (tmPro != null) tmPro.transform.rotation = Quaternion.identity;
    }


    private Vector3 GetCurrentPosition() {
        float normalizedDistance = _distanceTraveled.Value / _totalLength.Value;
        float t = GetTFromArcLength(normalizedDistance);
        return GetPoint(t);
    }
}