using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    
    public AudioSource coinSound;
    public AudioSource jumpSound;
    public AudioSource deathSound;
    public AudioSource goalSound;
    public float cameraDistanceZ = 10;
    public float cameraDistanceX = 0;
    public float cameraDistanceY = 8;

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

    void Start() {
        CameraFollowPlayer();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Coin")) {
            GameManager.instance.AddScore(1);
            coinSound.Play();
            Destroy(other.gameObject);
        } else if (other.CompareTag("Enemy")) {
            print("Game Over!");
            deathSound.Play();
            GameManager.instance.GameOver();
        } else if (other.CompareTag("Goal")) {
            goalSound.Play();
            GameManager.instance.NextLevel();
        }
    }
    
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

    private void checkIfFallingInTheVoid() {
        if (transform.position.y < -3) {
            GameManager.instance.GameOver();
        }
    }

    void FixedUpdate() {
        CameraFollowPlayer();
        checkIfFallingInTheVoid();
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

    protected void CameraFollowPlayer() {
        Vector3 camPos = transform.position;
        camPos.z = transform.position.z + cameraDistanceZ;
        camPos.x = transform.position.x + cameraDistanceX;
        camPos.y = transform.position.y + cameraDistanceY;
        Camera.main.transform.position = camPos;
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

        if (Camera.main != null) {
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
            jumpSound.Play();
        }
    }

   // private void FixedUpdate() {
   //     walkHandler();
   //     jumpHandler();
   // }

    /*private void walkHandler() {
        var modifiedSpeed = walkingSpeed;
        if (isJumped) {
            modifiedSpeed /= 3;
        }

        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        //rb.AddForce(deltaVector, ForceMode.VelocityChange);
        if ((Math.Abs(h) > 0.001 || Math.Abs(v) > 0.001) && rb.velocity.magnitude <= maxSpeed) {
            var deltaVector = new Vector3(getMovement(h, modifiedSpeed), 0, getMovement(v, modifiedSpeed));
            
            rb.AddForce(deltaVector, ForceMode.VelocityChange);
        }
    }

    private float getMovement(float vl, float ws) {
        if (Math.Abs(vl) < 0.001) return 0;
        return (vl > 0 ? -1 : 1) * Time.deltaTime * ws;
    }

    private void jumpHandler() {
        var yAxis = Input.GetAxis("Jump");
        isJumped = !isGrounded();
        if (yAxis > 0) {
            if (isJumped) return;
            isJumped = true;
            var deltaVector = new Vector3(0, yAxis * jumpingSpeed, 0);
            rb.AddForce(deltaVector, ForceMode.Impulse);
            jumpSound.Play();
        }
    }

    private bool isGrounded() {
        var corner1 = getBottomCorner(size.x, size.z);
        var corner2 = getBottomCorner(-size.x, size.z);
        var corner3 = getBottomCorner(size.x, -size.z);
        var corner4 = getBottomCorner(-size.x, -size.z);
        return Physics.Raycast(corner1, -Vector3.up, 0.02f) ||
               Physics.Raycast(corner2, -Vector3.up, 0.02f) ||
               Physics.Raycast(corner3, -Vector3.up, 0.02f) ||
               Physics.Raycast(corner4, -Vector3.up, 0.02f);
    }

    private Vector3 getBottomCorner(float sizeX, float sizeZ) {
        return transform.position + new Vector3(sizeX / 2 + 0.001f, -size.y / 2 + 0.01f, sizeZ / 2 + 0.001f);
    }*/
}