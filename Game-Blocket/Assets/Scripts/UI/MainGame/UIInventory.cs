using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>
/// Sets the Changable Settings for the Main-Game UI
/// </summary>
public class UIInventory : MonoBehaviour
{
	public static UIInventory Singleton { get; private set; }

	#region General Settings
	[Header("Settings: Changable - Backgrounds")]
	public Color inventoryBackground;
	/// <summary>Space to Edge of the GameObject</summary>
	public int spaceToBorderX, spaceToBorderY;
	public GameObject atHandSlot;
	#endregion

	#region InventorySlotSettings
	[Header("Settings: Slots - Inventory")]
	/// <summary>Count of Rows and Coloums</summary>
	public byte rows = 5, coloums = 8;

	/// <summary>Space between Rows and Colums</summary>
	public int rowspacingInvSlot = 15, colspacingInvSlot = 15;


	#endregion

	#region InventoryPlayerInfoSettings
	[Header("Settings: Playerinfo - Inventory")]
	public byte countAccessoireSlots, rowspacingPlayerInfo;

	///<summary>
	///AccessoiresSlots are inherted from the <see cref="prefabItemSlot"/> thats why they have to be scaled smaller<br></br>
	///Min: 1, Max: 100 in percent
	/// </summary>
	[Range(1, 100)]
	public byte scaleIndicator;

	[Header("Static: Playerinfo - Inventory")]
	///<summary>ArmorSlots: Top - Mid - Down</summary>
	public List<UIInventorySlot> armorSlots = new List<UIInventorySlot>(3), accessoiresSlots = new List<UIInventorySlot>();
	public GameObject accessoiresParent;
	#endregion

	#region InventoryPlayerStatsSettings
	[Header("Static: Player Stats")]
	public Text heartStat, shieldStat, swordStat;
	[Header("Static: Description")]
	public Text titleText, descriptonText;
	#endregion

	#region Static Resources !DO NOT TOUCH!
	[Header("Static: General")]
	///<summary>Gameobject from Inspector</summary>
	public GameObject uiParent, slotField, uiHud, hudslotfieldParent,ovenParent,backgroundParent; //Oven doesn't work
	/// <summary>Image from Inspector</summary>
	public Image inventoryBackgroundImage;
	public GameObject buffDisplayingParent,deathScreen;

	[Header("Other Prefabs")]
	public RectTransform chatParent;
	public InputField chatInput;
	public ScrollViewHandler chatHistoryView;
	/// <summary>Prefab from Inspector</summary>
	public GameObject prefabItemSlot, _slotOptions, loadingScreen, craftingInterfacePlaceholder,handCrafting;
	/// <summary>
	/// Original position of the Inventory
	/// </summary>
	private Vector2 uiParentPosition;
	#endregion

	/// <summary><see cref="global::Inventory"/></summary>
	private Inventory Inventory => Inventory.Singleton;

	#region Initzializement

	/// <summary>
	/// Initzialize the Inventory;
	/// </summary>
	private void InitUI()
	{
		InitSlots();
		InitHudSlots();
		//InitPlayerInfo();
		InitAtHand();
	}

	/// <summary>
	/// Initzialize the AtHand<see cref="UIInventorySlot"/>
	/// </summary>
	private void InitAtHand()
	{
		atHandSlot = Instantiate(prefabItemSlot, UIInventory.Singleton.transform);
		atHandSlot.name = "SlotAtHand";
		atHandSlot.SetActive(false);
		Destroy(atHandSlot.GetComponentInChildren<Image>());
		Destroy(atHandSlot.GetComponentInChildren<Button>());

		UIInventorySlot atHandUISlot = atHandSlot.GetComponent<UIInventorySlot>();
		Inventory.atHand = atHandUISlot;
		atHandUISlot.itemImage.raycastTarget = false;

		RectTransform atHandT = atHandSlot.GetComponent<RectTransform>();
		atHandT.localScale = new Vector3(0.8f, 0.8f, 1);
		//Inventory.atHandVector = new Vector2(-atHandT.rect.width / 2, atHandT.rect.height / 2);
	}

	/// <summary>
	/// Initzialize the PlayerInfoUI
	/// <br></br>Not used!
	/// </summary>
	private void InitPlayerInfo()
	{
		//TODO: Make dynamic
		///ArmorSlots
		//foreach(GameObject go1 in armorSlots)
		//	go1.transform.localPosition = new Vector3Int(spaceToBorderX + (int)go1.transform.localPosition.x, (int)go1.transform.localPosition.y, 1);

		///AccessoiresSlot
		if (!accessoiresParent)
			Debug.LogError("AccessoiresParent not Initzialized");
		for (int i = 0; i < countAccessoireSlots; i++)
		{
			float height = prefabItemSlot.GetComponent<RectTransform>().rect.height * scaleIndicator / 100;
			float accSlotY = accessoiresParent.transform.position.y + (height * i + rowspacingPlayerInfo * i);
			Vector3 posSlotNow = new Vector3(accessoiresParent.transform.position.x, accSlotY, 1);
			GameObject aGo = Instantiate<GameObject>(prefabItemSlot, posSlotNow, Quaternion.identity, accessoiresParent.transform);
			RectTransform aGoRT = aGo.GetComponent<RectTransform>();
			aGoRT.localScale = new Vector3(scaleIndicator / 100, scaleIndicator / 100, 1);
			aGo.name = $"Accessoire Slot {i}";
			//accessoiresSlots.Add(aGo);
		}
	}

	/// <summary>
	/// Initzialize the ItemSlotField
	/// </summary>
	private void InitSlots()
	{
		//Get With and height from the Prefab
		float prefW = prefabItemSlot.GetComponent<RectTransform>().rect.width,
			prefH = prefabItemSlot.GetComponent<RectTransform>().rect.height;
		//Go through every Slot
		for (byte a = 0; a < rows; a++)
		{
			for (byte b = 0; b < coloums; b++)
			{
				//Calc the !absolute Pos
				float itemSlotX = slotField.transform.position.x + prefW * b + spaceToBorderX + colspacingInvSlot * b;
				float itemSlotY = slotField.transform.position.y - (prefH * a + spaceToBorderY + rowspacingInvSlot * a);
				//Instantiate the Gameobject
				GameObject itemSlot = Instantiate(prefabItemSlot, new Vector3Int((int)(itemSlotX), (int)(itemSlotY), 1), Quaternion.identity, slotField.transform);
				if (a == 0)
					itemSlot.GetComponent<UIInventorySlot>().isHotBarSlot = true;
				//Name it
				itemSlot.name = $"Slot {a} - {b}";
				//Add to Inventory Logic
				Inventory.InvSlots.Add(itemSlot.GetComponent<UIInventorySlot>());
			}
		}
	}

	/// <summary></summary>

	private void InitHudSlots() {
		//Get With and height from the Prefab
		float prefW = prefabItemSlot.GetComponent<RectTransform>().rect.width;
		//Go through every Slot
		for (byte a = 0; a < coloums; a++)
		{
			//Calc the !absolute Pos
			float itemSlotX = hudslotfieldParent.transform.position.x + prefW * a + 50 + colspacingInvSlot * 0.2f * a;
			float itemSlotY = hudslotfieldParent.transform.position.y - 15;
			//Instantiate the Gameobject
			GameObject itemSlot = Instantiate(prefabItemSlot, new Vector3Int((int)(itemSlotX), (int)(itemSlotY), 1), Quaternion.identity, hudslotfieldParent.transform);
			itemSlot.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 1);
			//Name it
			itemSlot.name = $"HudSlot {a}";
			//Specify that it is a HotbarSlot
			itemSlot.GetComponent<UIInventorySlot>().isHotBarSlot = true;
			itemSlot.GetComponent<UIInventorySlot>().parent = Inventory.InvSlots[a];
			//Add to Inventory Logic
			Inventory.HudSlots.Add(itemSlot.GetComponent<UIInventorySlot>());
		}
	}

	public void Init(){
		if (Inventory == null)
			Debug.LogError("Inventory not found!");
		ReloadSettings();
		InitUI();
		Inventory.ArmorSlots = armorSlots;
		Inventory.AccessoiresSlots = accessoiresSlots;
		InventoryOpened = false;
		Inventory.SelectedSlot = 0;
		
		foreach(Canvas canvas in GetComponentsInChildren<Canvas>()){
			canvas.worldCamera = Camera.main;
        }
		
	}
	#endregion

	#region UnityMethods
	private void Start()
	{
		craftingInterfacePlaceholder.SetActive(false);
	}

	/// <summary>"Reload" at the beginning</summary>
	public void Awake()
	{
		Singleton = this;
		name = "UI";
	}

	public void Update()
	{
		if (GameManager.State != GameState.INGAME)
			return;

		if (Input.GetKeyDown(GameManager.SettingsProfile.InventoryKey) || Input.GetKeyDown(KeyCode.Joystick1Button3))
		{
			hudslotfieldParent.SetActive(InventoryOpened);
			uiHud.SetActive(InventoryOpened);
			//Inv
			InventoryOpened = !InventoryOpened;
			if (!InventoryOpened)
				SynchronizeToHotbar();

			//Carfting?
			///TODO: CLEANUP
			if (InventoryOpened){
				if (GlobalVariables.ActivatedCraftingInterface != null)
				{
					craftingInterfacePlaceholder.SetActive(true);
					GlobalVariables.ActivatedCraftingInterface.SetActive(false);
					uiParentPosition = uiParent.transform.position;
					uiParent.transform.position = new Vector2(uiParent.transform.position.x + 35, uiParent.transform.position.y);
					craftingInterfacePlaceholder.GetComponent<Image>().sprite = GlobalVariables.ActivatedCraftingInterface.GetComponent<Image>().sprite;
					CraftingStation.InstatiateCraftingInterface(prefabItemSlot, craftingInterfacePlaceholder, ItemAssets.Singleton.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(craftingInterfacePlaceholder.GetComponent<Image>().sprite)), ItemAssets.Singleton.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(craftingInterfacePlaceholder.GetComponent<Image>().sprite)).Slotwidth, ItemAssets.Singleton.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(craftingInterfacePlaceholder.GetComponent<Image>().sprite)).Slotheight);
					CraftingStation.RenewRecommendations(new Craftable[2],craftingInterfacePlaceholder);
                }
                else
                {
					if(ovenOpened)
                    {
						ovenParent.SetActive(true);
						OvenCalculation.Singleton.gameObject.SetActive(true);
						uiParentPosition = uiParent.transform.position;
						uiParent.transform.position = new Vector2(uiParent.transform.position.x + 35, uiParent.transform.position.y);
					}
					if (handCrafting.GetComponentInChildren<UIInventorySlot>() == null)
					{
						CraftingStation.InstatiateCraftingInterface(prefabItemSlot, handCrafting, ItemAssets.Singleton.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(handCrafting.GetComponent<Image>().sprite)), ItemAssets.Singleton.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(handCrafting.GetComponent<Image>().sprite)).Slotwidth, ItemAssets.Singleton.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(handCrafting.GetComponent<Image>().sprite)).Slotheight);
						CraftingStation.RenewRecommendations(new Craftable[2], handCrafting);
					}
				}
			}else{
				//TODO: Optional things to do...
				//uiParent.SetActive(false);

				if (GlobalVariables.ActivatedCraftingInterface != null)
				{
					GlobalVariables.ActivatedCraftingInterface.SetActive(true);
					foreach(UIInventorySlot uis in craftingInterfacePlaceholder.GetComponentsInChildren<UIInventorySlot>())
					{
						if (uis.ItemID != 0)
						{
							Inventory.AddItem(uis.ItemID, uis.ItemCount, out ushort itemCountNotAdded);
						}
                        Destroy(uis.gameObject);
					}
					uiParent.transform.position = uiParentPosition;
					craftingInterfacePlaceholder.SetActive(false);
                }
				//uiHud.SetActive(!InventoryOpened);

			}
		}

		//Chat
		if ((Input.GetKeyDown(GameManager.SettingsProfile.ChatKey)|| Input.GetKeyDown(KeyCode.Joystick1Button12)) && !ChatOpened)
		{
			ChatOpened = true;
			Debug.Log("Open");
			GameManager.SettingsProfile.KeyValues = GameManager.SettingsProfile.LockKeysExceptSpecified(GameManager.SettingsProfile.ChatKey).ToList();
			Debug.Log(GameManager.SettingsProfile.InventoryKey);
		}
		if (ChatOpened){
			if (Input.GetKeyDown(KeyCode.Escape))
            {
				ChatOpened = false;
				GameManager.SettingsProfile.KeyValues = SettingsProfile.FillKeyValues();
			}
			if (Input.GetKeyDown(KeyCode.Return)){ //Return = Enter
				ConsoleHandler.Handle(chatInput.text);
				ChatOpened = false;
				GameManager.SettingsProfile.KeyValues = SettingsProfile.FillKeyValues();
			}
		}

		//Scroll
		if (Input.mouseScrollDelta.y != 0 && !ChatOpened && !UIInventory.Singleton.deathScreen.activeSelf)
		{
			float val = Input.mouseScrollDelta.y;
			if (val < 0)
				if (Inventory.SelectedSlot == Inventory.HudSlots.Count - 1)

					Inventory.SelectedSlot = 0;
				else
					Inventory.SelectedSlot += 1;
			else
				if (Inventory.SelectedSlot == 0)
				Inventory.SelectedSlot = (byte)(Inventory.HudSlots.Count - 1);
			else
				Inventory.SelectedSlot -= 1;
		}
	}
	#endregion

	/// <summary>Reloads all UI Settings</summary>
	public void ReloadSettings()
	{
		if (!uiParent)
			Debug.LogError("Parent from UI is NULL!");
		//Set the Backgroung from the UI-Inventory
		inventoryBackgroundImage.color = inventoryBackground;
	}

	/// <summary>Returns and sets if the inventory should open</summary>
	public bool InventoryOpened
	{
		get => inventoryOpened;
		set
		{
			inventoryOpened = value;
			ChatOpened = false;
			if (value)
			{
				//TODO: Optional things to do... Example: Lock Mouseplace or break
				uiParent.SetActive(true);
				
			}
			else
			{
				//TODO: Optional things to do...
				uiParent.SetActive(false);
			}

		}
	}
	private static bool inventoryOpened;

	public bool ovenOpened=false;

	public bool ChatOpened
	{
		get => chatOpened;

		set{
			if (InventoryOpened)
				return;
			chatOpened = value;
			if (value){
				chatInput.text = "";
				chatParent.gameObject.SetActive(true);
				chatInput.Select();
			}else{
				//TODO: Optional things to do...
				chatParent.gameObject.SetActive(false);
			}

		}
	}
	private static bool chatOpened;

	public string DescriptionText
	{
		get { return descriptonText.text; }
		set { descriptonText.text = value; }
	}

	public string TitleText
	{
		get { return titleText.text; }
		set { titleText.text = value; }
	}

	#region Bidirectional Sync

	/// <summary>
	/// Synchronizes Hotbar State of Slots Row 1
	/// </summary>

	/// <summary>
	/// Synchronizes Inventory State of Slots Row 1
	/// </summary>

	public void SynchronizeToHotbar() {
		for(int i = 0; i < Inventory.HudSlots.Count; i++){ 
			Inventory.HudSlots[i].ItemID = Inventory.InvSlots[i].ItemID;
			Inventory.HudSlots[i].ItemCount = Inventory.InvSlots[i].ItemCount;	
		}
		PlayerVariables.Singleton.ReloadItemInHand();
	}
	#endregion

	public void DeathScreen()
    {
		deathScreen.SetActive(true);
		uiHud.SetActive(false);
		InventoryOpened = false;
		Movement.Singleton.PlayerLocked = true;
    }

	public void RespawnUI()
    {
		deathScreen.SetActive(false);
		uiHud.SetActive(true);
		InventoryOpened = false;
		PlayerHealth.Singleton.maxHealth = 100;
		PlayerHealth.Singleton.CurrentHealth = 100;
		Movement.Singleton.PlayerLocked = false;
	}
}
