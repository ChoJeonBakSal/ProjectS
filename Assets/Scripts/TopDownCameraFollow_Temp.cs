using UnityEngine;

public class TopDownCameraFollow_Temp : MonoBehaviour
{
    public Transform target; // ���� ��� (�÷��̾�)
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -8.69f); // ī�޶�� �÷��̾� ���� ������� ��ġ

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = desiredPosition;

        // ī�޶�� �÷��̾ �����ٺ��� ������ ����
        transform.LookAt(target.position + Vector3.up * offset.y);

        // ī�޶� ȸ�� �� ����
        transform.rotation = Quaternion.Euler(45, 0, 0); // ���ϴ� ������ ����
    }
}
