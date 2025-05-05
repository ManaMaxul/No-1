using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class Item
    {
        public ItemType type;
        public GameObject prefab;
        public float damage;
        public float defense;
        public string itemName;
        public string description;

        public Item(ItemType type, GameObject prefab = null, float damage = 0f, float defense = 0f, string itemName = "", string description = "")
        {
            this.type = type;
            this.prefab = prefab;
            this.damage = damage;
            this.defense = defense;
            this.itemName = itemName;
            this.description = description;
        }
    }

    public enum ItemType
    {
        Espada,
        EspadaLarga,
        EspadaPesada,
        Escudo,
        EscudoGrande,
        Arco,
        Pistola,
        Bomba
    }

    [SerializeField] private List<Item> itemLibrary = new List<Item>();
    [SerializeField] private Transform leftHandTransform;
    [SerializeField] private Transform rightHandTransform;
    
    private Item _leftHandItem;
    private Item _rightHandItem;
    private GameObject _leftHandObject;
    private GameObject _rightHandObject;
    private List<Item> weaponInventory = new List<Item>();
    private int currentWeaponIndex = -1;

    public Item leftHandItem => _leftHandItem;
    public Item rightHandItem => _rightHandItem;

    void Start()
    {
        if (leftHandTransform == null || rightHandTransform == null)
        {
            Debug.LogError("Por favor asigna los puntos de anclaje para las manos en el Inspector.");
        }

        // Inicializar con un escudo básico en la mano derecha
        Item shield = itemLibrary.Find(i => i.type == ItemType.Escudo);
        if (shield != null)
        {
            _rightHandItem = shield;
            UpdateHandVisuals(false);
        }
    }

    void Update()
    {
        // Cambiar armas con Q y E
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleWeapon(-1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            CycleWeapon(1);
        }

        // Intercambiar armas entre manos con F
        if (Input.GetKeyDown(KeyCode.F))
        {
            SwapHands();
        }
    }

    public bool AddItem(Item item)
    {
        if (IsWeapon(item.type))
        {
            if (weaponInventory.Count < 2)
            {
                weaponInventory.Add(item);
                if (_leftHandItem == null)
                {
                    _leftHandItem = item;
                    currentWeaponIndex = weaponInventory.Count - 1;
                    UpdateHandVisuals(true);
                }
                Debug.Log($"{item.itemName} añadido al inventario de armas.");
                return true;
            }
            else
            {
                Debug.Log("Inventario de armas lleno (máximo 2 armas).");
                return false;
            }
        }
        return false;
    }

    private void CycleWeapon(int direction)
    {
        if (weaponInventory.Count == 0) return;

        currentWeaponIndex = (currentWeaponIndex + direction + weaponInventory.Count) % weaponInventory.Count;
        _leftHandItem = weaponInventory[currentWeaponIndex];
        UpdateHandVisuals(true);
        Debug.Log($"Arma cambiada a: {_leftHandItem.itemName}");
    }

    private void SwapHands()
    {
        if (_leftHandItem == null || _rightHandItem == null) return;

        Item temp = _leftHandItem;
        _leftHandItem = _rightHandItem;
        _rightHandItem = temp;

        UpdateHandVisuals(true);
        UpdateHandVisuals(false);
        Debug.Log("Armas intercambiadas entre manos");
    }

    private void UpdateHandVisuals(bool isLeftHand)
    {
        Transform targetHand = isLeftHand ? leftHandTransform : rightHandTransform;
        Item targetItem = isLeftHand ? _leftHandItem : _rightHandItem;
        ref GameObject targetObject = ref (isLeftHand ? ref _leftHandObject : ref _rightHandObject);

        if (targetObject != null) Destroy(targetObject);
        if (targetItem != null && targetItem.prefab != null && targetHand != null)
        {
            targetObject = Instantiate(targetItem.prefab, targetHand.position, targetHand.rotation, targetHand);
        }
    }

    private bool IsWeapon(ItemType item)
    {
        return item == ItemType.Espada || 
               item == ItemType.EspadaLarga || 
               item == ItemType.EspadaPesada || 
               item == ItemType.Arco || 
               item == ItemType.Pistola || 
               item == ItemType.Bomba;
    }

    public void ClearInventory()
    {
        _leftHandItem = null;
        weaponInventory.Clear();
        currentWeaponIndex = -1;
        if (_leftHandObject != null) Destroy(_leftHandObject);
        if (_rightHandObject != null) Destroy(_rightHandObject);
        _rightHandItem = itemLibrary.Find(i => i.type == ItemType.Escudo);
        UpdateHandVisuals(false);
        Debug.Log("Inventario vaciado (escudo restaurado)");
    }
}