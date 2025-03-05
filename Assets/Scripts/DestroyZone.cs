using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    /*조건 : DestroyZone과 Block에 Collider가 있어야한다
             둘 중 하나는 Rigid Body가 있어야한다.*/

    private void OnCollisionEnter(Collision collision)  //남은 파편들 없애기
    {
        if (collision.gameObject.name.Equals("Rubble"))  //collision이 rubble과 같으면
        {
            Destroy(collision.gameObject);  //충돌한 Object를 파괴한다
        }
    }
}
