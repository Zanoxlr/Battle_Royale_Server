using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public float health = 100;
    public int weaponId = 0;
    public int ammo = 0;
    public CharacterController controller;
    public float gravity = -9.81f;
    public float moveSpeed = 10f;
    public float jumpSpeed = 5f;
    //
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public int ammoInMag = 0;
    public int maxAmmoInMag = 0;
    public GameObject[] instatiateObjects; 
    public Transform camPos;
    float pickUpRange = 3f;
    bool isGood = true;
    bool shooting = false;
    bool landed = false;
    bool readyToReload = true;
    float stepInterval = 0;
    public int killCount = 0;
    //
    private bool[] inputs;
    private float yVelocity = 0;
    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }
    public void Initialize(int _id, string _username, float _health, int _weaponId, int _ammo)
    {
        id = _id; ;
        username = _username;
        health = _health;
        weaponId = _weaponId;
        ammo = _ammo;

        inputs = new bool[8];
    }
    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }
        if (inputs[5])
        {
            WantToShoot();
        }
        if (inputs[6])
        {
            PickingUpPickables();
        }
        if (inputs[7])
        {
            StartCoroutine(Reload());
        }
        if (isGood == false)
        {
            if (health > 0)
            {
                health -= 3 * Time.fixedDeltaTime;
                ServerSend.PlayerHealth(this);
            }
            else
            {
                Died();
            }
        }

        Move(_inputDirection);
        ProgressStepCycle(moveSpeed, _inputDirection);
    }
    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            if (!landed)
            {
                ServerSend.PlayerSounds(transform.position, 3);
                landed = true;
            }
            yVelocity = 0f;
            if (inputs[4])
            {
                ServerSend.PlayerSounds(transform.position, 2);
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }
    private void ProgressStepCycle(float speed, Vector2 _inputDirection)
    {
        if (controller.velocity.sqrMagnitude > 0 && (_inputDirection.x != 0 || _inputDirection.y != 0))
        {
            stepInterval += Time.fixedDeltaTime / speed;
        }
        if (stepInterval >= 1.5f)
        {
            PlayFootStepAudio();
            stepInterval = 0;
        }
    }
    private void PlayFootStepAudio()
    {
        if (!controller.isGrounded)
        {
            return;
        }
        int num = Random.Range(0, 1);
        ServerSend.PlayerSounds(transform.position, num);
    }
    /// <summary>Updates the player input with newly received input.</summary>
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
    void Shoot()
    {
        ServerSend.PlayerSounds(transform.position, 3 + weaponId);
        ammoInMag -= 1;
        ServerSend.PlayerAmmoInMag(this);

        RaycastHit hit;
        if (Physics.Raycast(camPos.position, camPos.forward, out hit, range))
        {
            Player target = hit.transform.GetComponent<Player>();
            if (target != null)
            {
                float health = target.TakeDamage(damage, username, weaponId);
                if (health <= 0)
                {
                    killCount += 1;
                    ServerSend.PlayerKillCount(id, killCount);
                }
            }
        }
    }
    void PickingUpPickables()
    {
        RaycastHit hit;
        if (Physics.Raycast(camPos.position, camPos.forward, out hit, pickUpRange))
        {
            IdOnItems weapons = hit.transform.GetComponent<IdOnItems>();

            if (weapons != null)
            {
                int currnetWeaponID = weapons.id;
                if (currnetWeaponID == 1 || currnetWeaponID == 2)
                {
                    if (weaponId != currnetWeaponID)
                    {
                        ServerSend.InstatiateObject(weaponId, transform.position);
                        Instantiate(NetworkManager.instance.guns[weaponId], transform.position, Quaternion.identity, NetworkManager.instance.InstatiateObj);
                        weaponId = currnetWeaponID;
                        ServerSend.PlayerWeapon(this);
                        WeaponStats();
                        ServerSend.PickableRemove(weapons.name);
                        Destroy(hit.transform.gameObject);
                    }
                }
                if (currnetWeaponID == 3)
                {
                    Destroy(hit.transform.gameObject);
                    ServerSend.PickableRemove(weapons.name);
                    ammo += hit.transform.GetComponent<IdOnItems>().ammo;
                    ServerSend.PlayerAmmo(this);
                }
            }
        }
    }
    IEnumerator Reload()
    {
        if (ammoInMag <= maxAmmoInMag && ammo > 0 && readyToReload)
        {
            int num = 7;
            StartCoroutine(ReloadSounds(num));
            yield return new WaitForSeconds(1);
            if (ammo + ammoInMag <= maxAmmoInMag)
            {
                ammoInMag += ammo;
                ammo = 0;
                ServerSend.PlayerAmmo(this);
                ServerSend.PlayerAmmoInMag(this);
            }
            else
            {
                ammo -= maxAmmoInMag - ammoInMag;
                ammoInMag = maxAmmoInMag;
                ServerSend.PlayerAmmo(this);
                ServerSend.PlayerAmmoInMag(this);
            }
        }
        else if (readyToReload)
        {
            int num = 6;
            StartCoroutine(ReloadSounds(num));
        }
    }
    public float TakeDamage(float amount, string _nameKiller, int weaponId)
    {
        if (health > 0)
        {
            health -= amount;
            ServerSend.PlayerHealth(this);
            ServerSend.PlayerSounds(transform.position, 8);
            return health;
        }
        else
        {
            ServerSend.KillFeed(_nameKiller, username, weaponId);
            Died();
            return health;
        }
    }
    void WantToShoot()
    {
        if(shooting == false && ammoInMag > 0 && (weaponId == 1 || weaponId == 2))
        {
            Shoot();
            StartCoroutine(Shooting());
        }
    }
    IEnumerator Shooting()
    {
        shooting = true;
        yield return new WaitForSeconds(fireRate);
        shooting = false;
    }
    void WeaponStats()
    {
        if (weaponId == 1)
        {
            damage = 10f;
            range = 100f;
            fireRate = 0.2f;
            ammoInMag = 0;
            maxAmmoInMag = 30;
        }
        else if (weaponId == 2)
        {
            damage = 25f;
            range = 100f;
            fireRate = 0.3f;
            ammoInMag = 0;
            maxAmmoInMag = 7;
        }
    }
    public void ResetValues()
    {
        health = 100;
        weaponId = 0;
        ammo = 15;
        ammoInMag = 0;
        ServerSend.PlayerAmmo(this);
        ServerSend.PlayerHealth(this);
        ServerSend.PlayerWeapon(this);
        ServerSend.PlayerAmmoInMag(this);
        ServerSend.PlayerKillCount(id, 0);
    }
    IEnumerator ReloadSounds(int indexOf)
    {
        readyToReload = false;
        ServerSend.PlayerSounds(transform.position, indexOf);
        yield return new WaitForSeconds(0.5f);
        readyToReload = true;
    }
    public void Died()
    {
        ServerSend.PlayerRemove(id);
        Server.MaxPlayers = NetworkManager.instance.playerCount;
        InstantiateGuns();
    }
    private void InstantiateGuns()
    {
        if (weaponId != 0)
        {
            Instantiate(instatiateObjects[weaponId], transform.position, Quaternion.Euler(0, 0, 0), null);
            ServerSend.InstatiateObject(weaponId, transform.position);
        }
        if (ammo != 0)
        {
            Instantiate(instatiateObjects[2], transform.position, Quaternion.Euler(0, 0, 0), null);
            ServerSend.InstatiateObject(2, transform.position);
        }
    }
    //out of zone
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Zone")
        {
            isGood = true;
            ServerSend.PPE(id, true);
        }
    }
    // In zone
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Zone")
        {
            isGood = false;
            ServerSend.PPE(id, false);
        }
    }
}
