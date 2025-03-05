using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    /*���� : DestroyZone�� Block�� Collider�� �־���Ѵ�
             �� �� �ϳ��� Rigid Body�� �־���Ѵ�.*/

    private void OnCollisionEnter(Collision collision)  //���� ����� ���ֱ�
    {
        if (collision.gameObject.name.Equals("Rubble"))  //collision�� rubble�� ������
        {
            Destroy(collision.gameObject);  //�浹�� Object�� �ı��Ѵ�
        }
    }
}
