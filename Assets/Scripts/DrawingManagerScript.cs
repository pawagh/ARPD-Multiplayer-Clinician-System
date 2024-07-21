using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class DrawingManagerScript : NetworkBehaviour
{
    [Networked] private NetworkBool IsDrawing { get; set; }
    [Networked] private Vector3 CurrentDrawingPosition { get; set; }

    [SerializeField] private NetworkPrefabRef _drawingPointPrefab;
    private List<NetworkObject> _currentStroke = new List<NetworkObject>();
    private List<List<NetworkObject>> _allStrokes = new List<List<NetworkObject>>();

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcStartDrawing(Vector3 startPosition)
    {
        if (!IsDrawing)
        {
            IsDrawing = true;
            CurrentDrawingPosition = startPosition;
            _currentStroke = new List<NetworkObject>();
            SpawnDrawingPoint(startPosition);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcStopDrawing()
    {
        if (IsDrawing)
        {
            IsDrawing = false;
            _allStrokes.Add(_currentStroke);
            _currentStroke = null;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcUpdateDrawing(Vector3 newPosition)
    {
        if (IsDrawing)
        {
            CurrentDrawingPosition = newPosition;
            SpawnDrawingPoint(newPosition);
        }
    }

    private void SpawnDrawingPoint(Vector3 position)
    {
        NetworkObject point = Runner.Spawn(_drawingPointPrefab, position, Quaternion.identity);
        _currentStroke.Add(point);
    }
}