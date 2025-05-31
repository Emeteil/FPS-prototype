using System;
using UnityEngine;

public class Life : MonoBehaviour
{
    private PlayerMovment playerMovment;
    private PlayerInteraction playerInteraction;
    private GrabUp grabUp;

    private Rigidbody rb;

    private float walkSpeed;
    private float sprintSpeed;
    private float crouchSpeed;

    [NonSerialized] public bool dead = false;

    [HideInInspector] public bool _block = false;
    private bool _ignorePause = false;

    public void Block(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = true;
        _block = true;
    }

    public void Unblock(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = false;
        _block = false;
    }

    private void Start()
    {
        playerMovment = GetComponent<PlayerMovment>();
        playerInteraction = GetComponent<PlayerInteraction>();
        grabUp = GetComponent<GrabUp>();

        walkSpeed = playerMovment.walkSpeed;
        sprintSpeed = playerMovment.sprintSpeed;
        crouchSpeed = playerMovment.crouchSpeed;
    }

    private void Update()
    {        
        if (_block) return;
        if (!dead) return;
        if (!Input.anyKeyDown) return;

        SceneChanger.Instance.ChangeScene();
    }

    public void Decelerate()
    {
        playerMovment.walkSpeed = walkSpeed / 1.5f;
        playerMovment.sprintSpeed = walkSpeed;
        playerMovment.crouchSpeed = crouchSpeed;
    }

    public void StronglyDecelerate()
    {
        playerMovment.walkSpeed = walkSpeed / 2f;
        playerMovment.sprintSpeed = walkSpeed / 2f;
        playerMovment.crouchSpeed = crouchSpeed / 1.5f;
    }

    public void ResetDecelerate()
    {
        playerMovment.walkSpeed = walkSpeed;
        playerMovment.sprintSpeed = sprintSpeed;
        playerMovment.crouchSpeed = crouchSpeed;
    }

    public void Kill()
    {
        ControllerManager.Instance.SwitchToDefault(ControllerSwitchMode.Immediate);
        playerMovment.CrouchDown();
        playerMovment.Block();
        playerInteraction.Block();

        grabUp.ReleaseObject();
        grabUp.Block();

        dead = true;
    }
}
