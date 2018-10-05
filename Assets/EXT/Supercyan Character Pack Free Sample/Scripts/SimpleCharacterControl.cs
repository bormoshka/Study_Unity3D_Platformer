using UnityEngine;
using System.Collections.Generic;

public class SimpleCharacterControl : MonoBehaviour {
    protected enum ControlMode {
        Tank,
        Direct
    }

    [SerializeField] protected float m_moveSpeed = 2;
    [SerializeField] protected float m_turnSpeed = 200;
    [SerializeField] protected float m_jumpForce = 4;
    [SerializeField] protected Animator m_animator;
    [SerializeField] protected Rigidbody m_rigidBody;

    [SerializeField] protected ControlMode m_controlMode = ControlMode.Direct;

    protected float m_currentV = 0;
    protected float m_currentH = 0;

    protected readonly float m_interpolation = 10;
    protected readonly float m_walkScale = 0.33f;
    protected readonly float m_backwardsWalkScale = 0.16f;
    protected readonly float m_backwardRunScale = 0.66f;

    protected bool m_wasGrounded;
    protected Vector3 m_currentDirection = Vector3.zero;

    protected float m_jumpTimeStamp = 0;
    protected float m_minJumpInterval = 0.25f;

    protected bool m_isGrounded;
    protected List<Collider> m_collisions = new List<Collider>();

    protected void OnCollisionEnter(Collision collision) {
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < contactPoints.Length; i++) {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f) {
                if (!m_collisions.Contains(collision.collider)) {
                    m_collisions.Add(collision.collider);
                }

                m_isGrounded = true;
            }
        }
    }

    protected void OnCollisionStay(Collision collision) {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++) {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f) {
                validSurfaceNormal = true;
                break;
            }
        }

        if (validSurfaceNormal) {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider)) {
                m_collisions.Add(collision.collider);
            }
        } else {
            if (m_collisions.Contains(collision.collider)) {
                m_collisions.Remove(collision.collider);
            }

            if (m_collisions.Count == 0) {
                m_isGrounded = false;
            }
        }
    }

    protected void OnCollisionExit(Collision collision) {
        if (m_collisions.Contains(collision.collider)) {
            m_collisions.Remove(collision.collider);
        }

        if (m_collisions.Count == 0) {
            m_isGrounded = false;
        }
    }

    void Update() {
        m_animator.SetBool("Grounded", m_isGrounded);

        switch (m_controlMode) {
            case ControlMode.Direct:
                DirectUpdate();
                break;

            case ControlMode.Tank:
                TankUpdate();
                break;

            default:
                Debug.LogError("Unsupported state");
                break;
        }

        m_wasGrounded = m_isGrounded;
    }

    protected void TankUpdate() {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        bool walk = Input.GetKey(KeyCode.LeftShift);

        if (v < 0) {
            if (walk) {
                v *= m_backwardsWalkScale;
            } else {
                v *= m_backwardRunScale;
            }
        } else if (walk) {
            v *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        transform.position += transform.forward * m_currentV * m_moveSpeed * Time.deltaTime;
        transform.Rotate(0, m_currentH * m_turnSpeed * Time.deltaTime, 0);

        m_animator.SetFloat("MoveSpeed", m_currentV);

        JumpingAndLanding();
    }

    protected void DirectUpdate() {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift)) {
            v *= m_walkScale;
            h *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero) {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.rotation = Quaternion.LookRotation(m_currentDirection);
            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

            m_animator.SetFloat("MoveSpeed", direction.magnitude);
        }

        JumpingAndLanding();
    }

    protected void JumpingAndLanding() {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && Input.GetKey(KeyCode.Space)) {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded) {
            m_animator.SetTrigger("Land");
        }

        if (!m_isGrounded && m_wasGrounded) {
            m_animator.SetTrigger("Jump");
        }
    }
}