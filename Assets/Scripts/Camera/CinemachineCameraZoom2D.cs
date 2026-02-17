using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class CinemachineCameraZoom2D : MonoBehaviour
{
    public static CinemachineCameraZoom2D Instance { get; private set; }

    private const float NORMAL_ORTHOGRAPHIC_SIZE = 10f;

    [SerializeField] private CinemachineCamera cinemachineCamera;

    private float targetOrthographicSize = 10f;

    private void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        float zoomSpeed = 2f;
        cinemachineCamera.Lens.OrthographicSize = 
           Mathf.Lerp(cinemachineCamera.Lens.OrthographicSize, targetOrthographicSize,zoomSpeed*Time.deltaTime);
    }

    public void SetTargetOrthographicSize(float targetOrthographicSize)
    {
        this.targetOrthographicSize = targetOrthographicSize;
    }

    public void SetNormalTargetOrthographicSize()
    {
        SetTargetOrthographicSize(NORMAL_ORTHOGRAPHIC_SIZE);
    }
}
