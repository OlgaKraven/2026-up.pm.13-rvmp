using UnityEngine;
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    private void LateUpdate()
    {
        var camera = GetComponent<Camera>();
        camera.orthographicSize = Mathf.Max(5.3f, 9f / camera.aspect);
    }
}
