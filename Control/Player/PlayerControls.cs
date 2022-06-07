using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tears_Of_Void.Combat;
using Tears_Of_Void.Resources;
using System;
using Tears_Of_Void.Stats;
using DuloGames.UI;
using UnityEngine.EventSystems;

namespace Tears_Of_Void.Control
{
    public class PlayerControls : MonoBehaviour, IModifierProvider
    {
        // inputs
        Vector2 inputs;
        [HideInInspector] public Vector2 inputNormalized;
        [HideInInspector] public float rotation;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        bool run = true;
        bool jump;
        [HideInInspector] public bool steer, autoRun;

        Vector3 velocity;
        float gravity = -15;
        float velocityY;
        float terminalVelocity = -25f;
        float fallMult;

        // Running
        public float baseSpeed;
        public float runSpeed = 4f;
        public float rotateSpeed = 1f;
        float currentSpeed;

        // Ground
        Vector3 forwardDirection, collisionPoint;
        float slopeAngle, forwardAngle, directionAngle, strafeAngle;
        float forwardMult, strafeMult; // mult = multiplier, used to keep our speed steady on all different slope angles, comes out of trigonometry
        Ray groundRay;
        RaycastHit groundHit;

        // Jumping
        bool jumping;
        float jumpSpeed;
        float jumpHeight = 4f;
        Vector3 jumpDirection;

        // Attacking
        CombatTarget target;
        SpellCaster skills;
        // Debug
        public bool showMoveDirection, showForwardDirection, showStrafeDirection, showFallNormal, showGroundRay;

        // References
        CharacterController controller;
        Health health;
        EnemyHealthDisplay displayHealth;
        BaseStats baseStats;
        public Transform groundDirection, fallDirection, moveDirection;
        [HideInInspector] public CameraController mainCamera;

        int key;
        private bool playerMoving;
        public bool canMove;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            baseStats = GetComponent<BaseStats>();
            displayHealth = GameObject.FindWithTag("EnemyHealthBar").GetComponent<EnemyHealthDisplay>();
            skills = GetComponent<SpellCaster>();
        }

        private void Start()
        {
            baseSpeed = baseStats.GetStat(Stat.MovementSpeed);
            //            print("baseSpeed = " + baseSpeed);
        }
        void Update()
        {

            if (health.IsDead()) return;

            InteractWithTarget();
            UpdateAnimator();

            if(canMove) return;

            GetInputs();
            Locomotion();

            if (InteractWithUI()) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                key = skills.GetSlotID(0);
                skills.AttemptSpecialAbility(key);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                key = skills.GetSlotID(1);
                skills.AttemptSpecialAbility(key);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                key = skills.GetSlotID(2);
                skills.AttemptSpecialAbility(key);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                key = skills.GetSlotID(3);
                skills.AttemptSpecialAbility(key);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                key = skills.GetSlotID(4);
                skills.AttemptSpecialAbility(key);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                key = skills.GetSlotID(5);
                skills.AttemptSpecialAbility(key);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                key = skills.GetSlotID(6);
                skills.AttemptSpecialAbility(key);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                key = skills.GetSlotID(7);
                skills.AttemptSpecialAbility(key);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                key = skills.GetSlotID(8);
                skills.AttemptSpecialAbility(key);
            }
        }

        private bool InteractWithUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        public AnimatorOverrideController GetAnimatorOverride()
        {
            return animatorOverrideController;
        }

        private void InteractWithTarget()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay(), 50f);
            foreach (RaycastHit hit in hits)
            {
                target = hit.transform.GetComponent<CombatTarget>();

                if (target == null) continue;

                if (!GetComponent<Fighter>().CanAttack(target.gameObject))
                {
                    continue;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    GetComponent<Fighter>().SetTarget(target.gameObject);
                    displayHealth.UpdateEnemyHealth();
                }

                if (Input.GetMouseButtonDown(1))
                {
                    GetComponent<Fighter>().SetTarget(target.gameObject);
                    GetComponent<Fighter>().Attack(target.gameObject);
                    displayHealth.UpdateEnemyHealth();
                }
            }

            if (Input.GetMouseButtonDown(1) && target != null)
            {
                GetComponent<Fighter>().Attack();
            }

        }

        private void UpdateAnimator()
        {
            float speed = Mathf.Abs(controller.velocity.magnitude);
            GetComponent<Animator>().SetFloat("forwardSpeed", speed);
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        public bool isPlayerMoving()
        {
            if (controller.velocity.magnitude > 0)
            {
                return playerMoving = true;
            }
            return playerMoving = false;
        }

        public void setCanMove(bool b){
            canMove = b;
        }

        void Locomotion()
        {
            GroundDirection();

            if (slopeAngle <= controller.slopeLimit && controller.isGrounded)
            {
                // Running / Walking
                currentSpeed = baseSpeed;
                if (run)
                {
                    currentSpeed *= runSpeed;

                    // Going slower backwards
                    if (inputNormalized.y < 0)
                    {
                        currentSpeed = currentSpeed / 2;
                    }
                }
            }
            else if (!controller.isGrounded || slopeAngle > controller.slopeLimit)
            {
                inputNormalized = Vector2.Lerp(inputNormalized, Vector2.zero, 0.025f);
                currentSpeed = Mathf.Lerp(currentSpeed, 0, 0.025f);
            }

            // Rotating
            Vector3 characterRotation = transform.eulerAngles + new Vector3(0, rotation * rotateSpeed, 0);
            transform.eulerAngles = characterRotation;

            // Jumping
            if (jump && controller.isGrounded && slopeAngle <= controller.slopeLimit)
            {
                Jump();
            }

            // Apply gravity to grounded and add to velocityY if it's lower than terminal velocity
            if (!controller.isGrounded && velocityY > terminalVelocity)
            {
                velocityY += gravity * Time.deltaTime;
            }
            else if (controller.isGrounded && slopeAngle > controller.slopeLimit) // Slide down hills
            {
                velocityY = Mathf.Lerp(velocityY, terminalVelocity, 0.25f);
            }

            // Applying inputs
            if (!jumping)
            {
                velocity = groundDirection.forward * inputNormalized.y * forwardMult + groundDirection.right * inputNormalized.x * strafeMult; // Applying movement direction inputs
                velocity *= currentSpeed; // Applying current move speed
                velocity += fallDirection.up * (velocityY * fallMult); // Gravity
            }
            else
            {
                velocity = jumpDirection * jumpSpeed + Vector3.up * velocityY;
            }


            // Moving controller
            controller.Move(velocity * Time.deltaTime);

            if (controller.isGrounded)
            {
                // Turn flag to false when we hit the ground
                if (jumping) { jumping = false; }

                // Stop gravity when grounded
                velocityY = 0;
            }
        }

        // We basically rotate our direction so that it matches the ground's angle
        void GroundDirection()
        {
            // Setting forwardDirection
            // Setting forwardDirection to controller position
            forwardDirection = transform.position;

            // Setting forwardDirection based on control input
            if (inputNormalized.magnitude > 0)
            {
                forwardDirection += transform.forward * inputNormalized.y + transform.right * inputNormalized.x;
            }
            else
            {
                forwardDirection += transform.forward;
            }

            // Setting groundDirection to look in the forwardDirection normal
            moveDirection.LookAt(forwardDirection);
            fallDirection.rotation = transform.rotation;
            groundDirection.rotation = transform.rotation;

            // Setting ground ray
            groundRay.origin = transform.position + collisionPoint + Vector3.up * 0.05f;
            groundRay.direction = Vector3.down;

            if (showGroundRay)
            {
                Debug.DrawLine(groundRay.origin, groundRay.origin + Vector3.down * 1.5f, Color.red);
            }

            forwardMult = 1; // When we are not on any kind of angle
            fallMult = 1; // When we are not on any kind of slope
            strafeMult = 1;

            if (Physics.Raycast(groundRay, out groundHit, 0.3f))
            {
                // Getting angles
                slopeAngle = Vector3.Angle(transform.up, groundHit.normal);
                directionAngle = Vector3.Angle(moveDirection.forward, groundHit.normal) - 90f; // -90f because the normal of the ground is 90degrees and our forwardAngle is off by 90

                if (directionAngle < 0 && slopeAngle <= controller.slopeLimit)
                {
                    forwardAngle = Vector3.Angle(transform.forward, groundHit.normal) - 90f; // Checking the forwardAngle against the slope
                    forwardMult = 1 / Mathf.Cos(forwardAngle * Mathf.Deg2Rad); // Applying the forward movement mult based on the forwardAngle. Have to convert degrees to radiants, this helps us take all slopes smoothly and at the same speed

                    // Setting movement direction based on forwardAngle
                    groundDirection.eulerAngles += new Vector3(-forwardAngle, 0, 0); // Rotating groundDirection X

                    strafeAngle = Vector3.Angle(groundDirection.right, groundHit.normal) - 90f; // Checking the strafeAngle against the slope
                    strafeMult = 1 / Mathf.Cos(strafeAngle * Mathf.Deg2Rad); // Applying the strafe movement mult based on the strafeAngle. Have to convert degrees to radiants, this helps us take all slopes smoothly and at the same speed
                    groundDirection.eulerAngles += new Vector3(0, 0, strafeAngle);
                }
                else if (slopeAngle > controller.slopeLimit)
                {
                    float groundDistance = Vector3.Distance(groundRay.origin, groundHit.point);

                    if (groundDistance <= 0.1f)
                    {
                        fallMult = 1 / Mathf.Cos((90 - slopeAngle) * Mathf.Deg2Rad);

                        Vector3 groundCross = Vector3.Cross(groundHit.normal, Vector3.up);
                        fallDirection.rotation = Quaternion.FromToRotation(transform.up, Vector3.Cross(groundCross, groundHit.normal)); // aligning our fallDirection with the downwards direction of the normal
                    }
                }
            }

            DebugGroundNormals();
        }

        void Jump()
        {
            if (!jumping) { jumping = true; }

            // Set jump direction and speed
            jumpDirection = (transform.forward * inputs.y + transform.right * inputs.x).normalized;
            jumpSpeed = currentSpeed;

            velocityY = Mathf.Sqrt(-gravity * jumpHeight);
        }

        void GetInputs()
        {
            if (Input.GetButtonDown("AutoRun"))
            {
                autoRun = !autoRun;
            }

            // FORWARDS/BACKWARDS CONTROLS
            inputs.y = Axis(Input.GetButton("Forwards"), Input.GetButton("Backwards"));

            if (inputs.y != 0 && !mainCamera.autoRunReset)
            {
                autoRun = false;
            }

            if (autoRun)
            {
                inputs.y += 1;
                inputs.y = Mathf.Clamp(inputs.y, -1, 1);
            }

            // STRAFING CONTROLS
            inputs.x = Axis(Input.GetButton("StrafeRight"), Input.GetButton("StrafeLeft"));

            if (steer)
            {
                inputs.x += Axis(Input.GetButton("RotateRight"), Input.GetButton("RotateLeft"));
                inputs.x = Mathf.Clamp(inputs.x, -1, 1);
            }

            // ROTATION CONTROLS
            if (steer)
            {
                rotation = Input.GetAxis("Mouse X") * mainCamera.cameraSpeed;
            }
            else
            {
                rotation = Axis(Input.GetButton("RotateRight"), Input.GetButton("RotateLeft"));
            }

            // Toggle Run
            if (Input.GetButtonDown("WalkRun"))
            {
                run = !run;
            }

            // Jumping
            jump = Input.GetButton("Jump");

            inputNormalized = inputs.normalized;

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
        }

        public float Axis(bool pos, bool neg)
        {
            float axis = 0;

            if (pos)
            {
                axis += 1;
            }

            if (neg)
            {
                axis -= 1;
            }

            return axis;
        }

        void DebugGroundNormals()
        {
            Vector3 lineStart = transform.position + Vector3.up * 0.05f;

            if (showMoveDirection)
            {
                Debug.DrawLine(lineStart, lineStart + moveDirection.forward * 1.5f, Color.cyan);
            }

            if (showForwardDirection)
            {
                Debug.DrawLine(lineStart - groundDirection.forward * 1.5f, lineStart + groundDirection.forward * 1.5f, Color.blue);
            }

            if (showStrafeDirection)
            {
                Debug.DrawLine(lineStart - groundDirection.right * 1.5f, lineStart + groundDirection.right * 1.5f, Color.red);
            }

            if (showFallNormal)
            {
                Debug.DrawLine(lineStart, lineStart + fallDirection.up * 1.5f, Color.green);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            collisionPoint = hit.point; // Smoothens the transition of the groundDirection between slopes
            collisionPoint = collisionPoint - transform.position; // To make the collisionPoint follow the player even when jumping
        }

        public IEnumerable<float> GetAdditiveModifier(Stat stat)
        {
            if (stat == Stat.MovementSpeed)
            {
                yield return baseSpeed;
            }
        }
    }
}
