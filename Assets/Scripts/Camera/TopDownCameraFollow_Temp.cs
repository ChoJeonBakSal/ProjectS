using UnityEngine;

public class TopDownCameraFollow_Temp : MonoBehaviour
{
    public Transform target; // 따라갈 대상 (플레이어)
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -8.69f); // 카메라와 플레이어 간의 상대적인 위치

    [SerializeField] private float X = 45f;
    [SerializeField] private float y = 0f;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = desiredPosition;

        // 카메라는 플레이어를 내려다보는 방향을 유지
        transform.LookAt(target.position + Vector3.up * offset.y);

        // 카메라 회전 값 조정
        transform.rotation = Quaternion.Euler(X, y, 0); // 원하는 각도로 설정
    }
}
