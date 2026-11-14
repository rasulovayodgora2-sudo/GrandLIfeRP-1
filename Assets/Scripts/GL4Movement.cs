using UnityEngine;
namespace GrandLife {
    public class GL4AvatarMove : MonoBehaviour {
        public float speed = 6f;
        CharacterController cc; float gravity;
        void Awake(){cc=GetComponent<CharacterController>();}
        void Update(){
            Vector2 v=InputState.CurrentMove;
            if(GL4Game.Instance!=null) v+=GL4Game.Instance.MobileAxis;
            v=Vector2.ClampMagnitude(v,1f);
            Vector3 direction=new Vector3(v.x,0,v.y);
            if(direction.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),10*Time.deltaTime);
            gravity=cc.isGrounded?-1f:gravity-9.81f*Time.deltaTime;
            cc.Move((direction*speed+Vector3.up*gravity)*Time.deltaTime);
        }
    }
    public class GL4Drive : MonoBehaviour {
        public string kind="Седан";
        public bool controlled;
        public float speed;
        void Update(){
            if(!controlled)return;
            Vector2 axis=InputState.CurrentMove;
            if(GL4Game.Instance!=null)axis+=GL4Game.Instance.MobileAxis;
            axis=Vector2.ClampMagnitude(axis,1);
            bool heli=kind=="Вертолёт";
            float maxSpeed=heli?28f:kind=="Мотоцикл"?24f:kind=="Спорткар"?32f:19f;
            float desired=axis.y*maxSpeed;
            speed=Mathf.MoveTowards(speed,desired,(heli?9f:15f)*Time.deltaTime);
            if(Mathf.Abs(axis.x)>.01f)transform.Rotate(0,axis.x*(heli?65f:85f)*Time.deltaTime,0);
            transform.position+=transform.forward*speed*Time.deltaTime;
            if(heli){
                float vertical=(Input.GetKey(KeyCode.E)?1:0)-(Input.GetKey(KeyCode.Q)?1:0);
                if(GL4Game.Instance!=null)vertical+=GL4Game.Instance.MobileVertical;
                Vector3 p=transform.position;p.y=Mathf.Clamp(p.y+vertical*12f*Time.deltaTime,3f,95f);transform.position=p;
            }else{Vector3 p=transform.position;p.y=0.05f;transform.position=p;}
        }
    }
    public class GL4FollowCamera : MonoBehaviour {
        public Transform target;
        void LateUpdate(){
            if(!target)return;
            Vector3 focus=target.position+Vector3.up*1.5f;
            float distance=target.GetComponent<GL4Drive>()!=null?13f:8f;
            float height=target.GetComponent<GL4Drive>()!=null?6f:4.5f;
            Vector3 dest=focus-target.forward*distance+Vector3.up*height;
            transform.position=Vector3.Lerp(transform.position,dest,Mathf.Clamp01(6f*Time.deltaTime));
            transform.LookAt(focus);
        }
    }
}
