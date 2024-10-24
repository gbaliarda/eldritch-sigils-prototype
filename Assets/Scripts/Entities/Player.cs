using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Player : Actor, IBuffable
{
    public static Player Instance { get; private set; }

    private Vector2 _moveDirection;

    [SerializeField] private MonoBehaviour _weapon;
    [SerializeField] private MonoBehaviour _potion;
    [FormerlySerializedAs("_potionBuilding")] [SerializeField] private MonoBehaviour _shotgunBulletBuilding;
    [SerializeField] private MonoBehaviour _potionFactory;
    [SerializeField] private MonoBehaviour _chestBuilding;
    [SerializeField] private MonoBehaviour _slider;
    [SerializeField] private MonoBehaviour _extractor;
    [SerializeField] private ActorStats _baseStats;
    [SerializeField] private Transform _hotbarItems;
    [SerializeField] private Transform _buildHotbarItems;

    // SFX
    [SerializeField] private string deadSound = "PlayerDead";

    public bool BuildingMode => _buildingMode;
    public bool DeleteBuildingMode => _deleteBuildingMode;

    private bool _buildingMode = false;
    private bool _deleteBuildingMode = false;
    public Transform HotbarItems => _hotbarItems;
    public Transform BuildHotbarItems => _buildHotbarItems;
    public Rigidbody2D Rigidbody { get; private set; }

    protected List<IPotion> buffs;
    public List<IPotion> Buffs => buffs;

    #region KEYBINDINGS
    [Header("Keybindings")]
    [SerializeField] private KeyCode _shoot = KeyCode.Mouse0;
    [SerializeField] private KeyCode _interact = KeyCode.Mouse1;

    [SerializeField] private KeyCode _hotbarSlot1 = KeyCode.Alpha1;
    [SerializeField] private KeyCode _hotbarSlot2 = KeyCode.Alpha2;
    [SerializeField] private KeyCode _hotbarSlot3 = KeyCode.Alpha3;
    [SerializeField] private KeyCode _hotbarSlot4 = KeyCode.Alpha4;
    [SerializeField] private KeyCode _hotbarSlot5 = KeyCode.Alpha5;
    [SerializeField] private KeyCode _hotbarSlot6 = KeyCode.Alpha6;
    [SerializeField] private KeyCode _inventory = KeyCode.I;
    [SerializeField] private KeyCode _buildModeKey = KeyCode.Q;
    [SerializeField] private KeyCode _rotateBuilding = KeyCode.R;
    [SerializeField] private KeyCode _deleteBuilding = KeyCode.Z;
    #endregion

    #region PARAMS
    [Header("Params")]
    [SerializeField] private float _shootOriginDistance = 5f;
    #endregion

    public GameObject ObjectInHand => _objectInHand;

    private GameObject _objectInHand;

    private int _buildingRotation = 1;

    private GameObject _currentZone;

    public GameObject CurrentZone => _currentZone;
    [SerializeField] private GameObject grave;


    protected override void Awake()
    {
        if (Instance != null && this.gameObject != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        Rigidbody = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();

        buffs ??= new List<IPotion>();

        EventManager.Instance.OnHotbarItemSelect += OnHotbarItemSelect;

        stats = Instantiate(_baseStats);
        _currentZone = GameObject.Find("Base");
    }

    #region MOVEMENT_INPUT
    void InputMovement()
    {
        var moveX = Input.GetAxisRaw("Horizontal");
        var moveY = Input.GetAxisRaw("Vertical");

        _moveDirection = new Vector2(moveX, moveY).normalized;
    }

    public void SetCurrentZone(GameObject zone)
    {
        _currentZone = zone;
    }
    #endregion

    [SerializeField] private GameObject _levelPortal;
    [SerializeField] private GameObject _basePortal;

    protected override void Update()
    {
        if (isDead) return;
        InputMovement();

        if (Input.GetKey(_hotbarSlot1)) EventManager.Instance.EventHotbarSlotChange(0);
        if (Input.GetKey(_hotbarSlot2)) EventManager.Instance.EventHotbarSlotChange(1);
        if (Input.GetKey(_hotbarSlot3)) EventManager.Instance.EventHotbarSlotChange(2);
        if (Input.GetKey(_hotbarSlot4)) EventManager.Instance.EventHotbarSlotChange(3);
        if (Input.GetKey(_hotbarSlot5)) EventManager.Instance.EventHotbarSlotChange(4);
        if (Input.GetKey(_hotbarSlot6)) EventManager.Instance.EventHotbarSlotChange(5);
        if (Input.GetKeyDown(KeyCode.P)) Instantiate(_levelPortal, transform.position + transform.rotation * Vector3.up * 2, Quaternion.identity, CurrentZone.transform);
        if (Input.GetKeyDown(KeyCode.O)) Instantiate(_basePortal, transform.position + transform.rotation * Vector3.up * 2, Quaternion.identity, CurrentZone.transform);
        if (Input.GetKeyDown(_deleteBuilding))
        {
            if (_buildingMode)
            {
                _deleteBuildingMode = !_deleteBuildingMode;
                EventManager.Instance.EventDeleteBuildModeActive(_deleteBuildingMode);
            }
        }
        if (Input.GetKeyDown(_buildModeKey))
        {
            _buildingMode = !_buildingMode;
            _hotbarItems.gameObject.SetActive(!_buildingMode);
            _buildHotbarItems.gameObject.SetActive(_buildingMode);
            EventManager.Instance.EventBuildModeActive(_buildingMode);
        }
        if(Input.GetKeyDown(_rotateBuilding))
        {
            _buildingRotation += 1;
            if (_buildingRotation == 5)
                _buildingRotation = 1;
        }

        if (Input.GetKeyDown(_inventory))
        {
            if (InventoryManager.Instance.IsOpen)
                EventManager.Instance.EventCloseInventoryUI();
            else
                EventManager.Instance.EventOpenInventoryUI();
        }

        if (Input.GetKeyDown(_interact) && _buildingMode && _deleteBuildingMode)
        {
            var cam = Camera.main;
            if (!cam) return;

            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            int layerToIgnore = LayerMask.GetMask("Camera");
            int layerMask = ~layerToIgnore;
            var hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, layerMask);
            if (hit.collider != null)
            {
                var clickedObject = hit.collider.gameObject;
                Debug.Log(clickedObject);

                if (clickedObject.TryGetComponent<IDestroyable>(out var destroyable))
                {
                    destroyable.Destroy();
                }
            }
        }

        if (Input.GetKeyDown(_interact) && _buildingMode && !_deleteBuildingMode)
        {
            var cam = Camera.main;
            if(!cam) return;

            var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            int layerToIgnore = LayerMask.GetMask("Camera");
            int layerMask = ~layerToIgnore;
            var hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, layerMask);
            if (hit.collider != null)
            {
                Vector3 clickPosition = hit.point;
                var clickedObject = hit.collider.gameObject;

                if (clickedObject.TryGetComponent<Tilemap>(out var tilemap))
                {
                    var cellPosition = tilemap.WorldToCell(clickPosition);

                    if (TileManager.Instance.IsInteractable(cellPosition))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject() && _shotgunBulletBuilding.gameObject.activeSelf && _shotgunBulletBuilding.GetComponent<IBuilding>() != null)
                        {
                            TileManager.Instance.SetOccupied(cellPosition);
                            (_shotgunBulletBuilding as IBuilding).Build(tilemap.CellToWorld(cellPosition) + tilemap.cellSize / 2 + new Vector3(0, 0, -1), _buildingRotation);
                        }
                        if (!EventSystem.current.IsPointerOverGameObject() && _potionFactory.gameObject.activeSelf && _potionFactory.GetComponent<IBuilding>() != null)
                        {
                            TileManager.Instance.SetOccupied(cellPosition);
                            (_potionFactory as IBuilding).Build(tilemap.CellToWorld(cellPosition) + tilemap.cellSize / 2 + new Vector3(0, 0, -1), _buildingRotation);
                        }
                        if (_slider.gameObject.activeSelf && _slider.GetComponent<IBuilding>() != null)
                        {
                            TileManager.Instance.SetOccupied(cellPosition);
                            (_slider as IBuilding).Build(tilemap.CellToWorld(cellPosition) + tilemap.cellSize / 2, _buildingRotation);
                        }
                        if (_chestBuilding.gameObject.activeSelf && _chestBuilding.GetComponent<IBuilding>() != null)
                        {
                            TileManager.Instance.SetOccupied(cellPosition);
                            (_chestBuilding as IBuilding).Build(tilemap.CellToWorld(cellPosition) + tilemap.cellSize / 2 + new Vector3(0, 0, -1), _buildingRotation);
                        }
                        if(_extractor.gameObject.activeSelf && _extractor.GetComponent<IBuilding>() != null)
                        {
                            if (TileManager.Instance.IsResource(cellPosition))
                            {
                                var tile = TileManager.Instance.GetResourceTile(cellPosition);

                                TileManager.Instance.SetOccupied(cellPosition);
                                var go = (_extractor as IBuilding).Build(tilemap.CellToWorld(cellPosition) + tilemap.cellSize / 2 + new Vector3(0, 0, -1), _buildingRotation);
                                var extractor = go.GetComponent<ExtractorBuilding>();
                                extractor.Tile = tile;
                            }
                        }
                    }
                    /*if (TileManager.Instance.IsInteractable(cellPosition))
                    {
                        TileManager.Instance.SetInteractable(cellPosition);
                    }*/
                }

                //EventQueueManager.Instance.AddEvent(_cmdLeftClick);
            }
        }


        if (Input.GetKeyDown(_shoot) && !_buildingMode)
        {
            if (!EventSystem.current.IsPointerOverGameObject() && _weapon.gameObject.activeSelf && _weapon.GetComponent<IWeapon>() != null)
            {
                ShootWeapon();
            }
            if (_potion.gameObject.activeSelf && _potion.GetComponent<IPotion>() != null)
            {
                UsePotion();
            }
        }

        if (_buildingMode)
        {
            _objectInHand = GetFirstActiveChild(_buildHotbarItems);
        } else
        {
            _objectInHand = GetFirstActiveChild(_hotbarItems);
        }
    }

    private GameObject GetFirstActiveChild(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.activeSelf)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Rigidbody.velocity = _moveDirection * stats.MovementSpeed;
    }

    private void OnHotbarItemSelect(GameObject go)
    {

    }

    private void ShootWeapon()
    {
        var cam = Camera.main;
        if(!cam) return;

        Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        var dir = (mousePosition - (Vector2)transform.position).normalized;
        var origin = (Vector2)transform.position + _shootOriginDistance * dir;
        ((IWeapon)_weapon).Attack(origin, dir);
    }

    private void UsePotion()
    {
        ((IPotion)_potion).Buff();
    }

    public void AddBuff(IPotion potion)
    {
        if (isDead) return;
        stats.AddStats(potion.PotionStats);
        buffs.Add(potion);
    }

    public void RemoveBuff(IPotion potion)
    {
        if (isDead) return;
        stats.RemoveStats(potion.PotionStats);
        buffs.Remove(potion);
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;
        if (animator != null) animator.SetBool("Dead", true);

        // if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        DeathScreenUI.Instance.Popup();
    }

    public void Respawn()
    {
        buffs ??= new();

        stats = _baseStats;
        _currentZone = GameObject.Find("Base");
        isDead = false;
        life = MaxLife;
        _moveDirection = new Vector2(0, 0);

        transform.SetPositionAndRotation(Base.Instance.Spawner.position, transform.rotation);
        CinemachineConfiner2D _cinemachineConfiner = GameObject.Find("VirtualCamera").GetComponent<CinemachineConfiner2D>();
        if (_cinemachineConfiner != null && Base.Instance.CameraConfiner != null)
        {
            _cinemachineConfiner.m_BoundingShape2D = Base.Instance.CameraConfiner;
            CinemachineVirtualCamera vcam = _cinemachineConfiner.GetComponent<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.OnTargetObjectWarped(transform, Base.Instance.Spawner.position - transform.position);

                vcam.PreviousStateIsValid = false;
            }
        }

        if (TemporalLevel.Instance != null) Destroy(TemporalLevel.Instance.gameObject);
        if (LevelPortal.Instance != null) Destroy(LevelPortal.Instance.gameObject);
        if (BasePortal.Instance != null) Destroy(BasePortal.Instance.gameObject);
    }
    
    // Override the Die method from Actor to display the death animation
    protected override IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        GameObject graveInstance = Instantiate(grave, transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX(deadSound);

        Destroy(gameObject);
    }
}
