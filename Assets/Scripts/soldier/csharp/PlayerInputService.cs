using UnityEngine;

public class PlayerInputService : MonoSingleton<PlayerInputService>
{
    private Vector3 _moveDirection;

    private bool _isCrouching;
    private bool _isFiring;
    private bool _isJumping;
    private bool _isReloading;
    private bool _isAiming;
    private bool _isRunning;
    private bool _isPausing;

    public Vector3 moveDirection { get { return _moveDirection; } }
    public bool IsCrouching { get { return _isCrouching; } }
    public bool IsFiring { get { return _isFiring; } }
    public bool IsJumping { get { return _isJumping; } }
    public bool IsReloading { get { return _isReloading; } }
    public bool IsAiming { get { return _isAiming; } }
    public bool IsRunning { get { return _isRunning; } }
    public bool IsPausing { get { return _isPausing; } }

    private void Update()
    {
        _moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _isFiring = Input.GetButton("Fire1");
        _isAiming = Input.GetButton("Fire2");
        _isJumping = Input.GetButton("Jump");

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _isCrouching = !_isCrouching;
        }

        _isRunning = Input.GetKey(KeyCode.LeftShift);
        _isReloading = Input.GetKeyDown(KeyCode.R);

        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            _isPausing = !_isPausing;
        }
    }
}
