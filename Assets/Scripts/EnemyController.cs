using System;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    Vector3 initialPosition;

    public float speed = 1;
    public float speedModifier = 2;
    public float rangeY = 1;
    private int direction = 1;
   
    [SerializeField] protected Animator m_animator;

    // Use this for initialization
    void Start() {
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        
        float movementY = Time.deltaTime * direction * (direction < 0 ? speed * speedModifier : speed);
        float newY = transform.position.y + movementY;

        if (Math.Abs(newY - initialPosition.y) > rangeY || transform.position.y < 1) {
            direction *= -1;
            movementY *= -1;
        }

        if (direction > 0) {
            m_animator.SetTrigger("Idle");
        } else {
            m_animator.SetTrigger("Attack");
        }
        transform.position += new Vector3(0, movementY, 0);
        
    }
}