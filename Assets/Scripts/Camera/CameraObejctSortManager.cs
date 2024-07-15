using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraObejctSortManager : MonoBehaviour
{
    [SerializeField] private GameObject Human;
    [SerializeField] private GameObject Wolf;

    [SerializeField] private Camera camera;
    [SerializeField] private Camera Player_Camera;
    [SerializeField] private Camera Sub_Camera;


    //private void OnEnable()
    //{
    //    camera = Camera.main;
    //}

    private void Update()
    {
        if(!gameObject.activeSelf) return;

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
        float zPositionPlayer = Human.transform.position.z;
        float zPositionPlayerSub = Wolf.transform.position.z;

        if (zPositionPlayer > zPositionPlayerSub)
        {
            // Player가 Z축에서 더 높은 위치에 있으면 Player_Camera를 앞에 배치
            SetCameraStackOrder(Player_Camera, Sub_Camera);
        }
        else
        {
            // Player_Sub가 Z축에서 더 높은 위치에 있으면 Sub_Camera를 앞에 배치
            SetCameraStackOrder(Sub_Camera, Player_Camera);
        }
    }

    private void SetCameraStackOrder(Camera frontCamera, Camera backCamera)
    {
        var baseCameraData = camera.GetUniversalAdditionalCameraData();
        if (baseCameraData != null)
        {
            baseCameraData.cameraStack.Clear(); // 기존 스택 초기화
            baseCameraData.cameraStack.Add(frontCamera);
            baseCameraData.cameraStack.Add(backCamera);
        }
        else
        {
            Debug.LogError("Main camera does not have UniversalAdditionalCameraData.");
        }
    }
}
