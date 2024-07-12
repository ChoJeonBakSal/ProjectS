using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraObejctSortManager : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject Player_Sub;

    private Camera camera;
    [SerializeField] private Camera Player_Camera;
    [SerializeField] private Camera Sub_Camera;


    private void Start()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        UpdateCameraStackOrder();
    }

    public void ChangeCameraStackOrder(int oldIndex, int newIndex)
    {
        var baseCameraData = camera.GetUniversalAdditionalCameraData();
        if (baseCameraData != null && oldIndex < baseCameraData.cameraStack.Count && newIndex < baseCameraData.cameraStack.Count)
        {
            var cameraToMove = baseCameraData.cameraStack[oldIndex];
            baseCameraData.cameraStack.RemoveAt(oldIndex);
            baseCameraData.cameraStack.Insert(newIndex, cameraToMove);
        }
        else
        {
            Debug.LogError("Invalid indices for camera stack.");
        }
    }

    private void UpdateCameraStackOrder()
    {
        float zPositionPlayer = Player.transform.position.z;
        float zPositionPlayerSub = Player_Sub.transform.position.z;

        if (zPositionPlayer > zPositionPlayerSub)
        {
            // Player�� Z�࿡�� �� ���� ��ġ�� ������ Player_Camera�� �տ� ��ġ
            SetCameraStackOrder(Player_Camera, Sub_Camera);
        }
        else
        {
            // Player_Sub�� Z�࿡�� �� ���� ��ġ�� ������ Sub_Camera�� �տ� ��ġ
            SetCameraStackOrder(Sub_Camera, Player_Camera);
        }
    }
    private void SetCameraStackOrder(Camera frontCamera, Camera backCamera)
    {
        var baseCameraData = camera.GetUniversalAdditionalCameraData();
        if (baseCameraData != null)
        {
            baseCameraData.cameraStack.Clear(); // ���� ���� �ʱ�ȭ
            baseCameraData.cameraStack.Add(frontCamera);
            baseCameraData.cameraStack.Add(backCamera);
        }
        else
        {
            Debug.LogError("Main camera does not have UniversalAdditionalCameraData.");
        }
    }
}
