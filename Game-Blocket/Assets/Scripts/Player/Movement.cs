using System;

using UnityEngine;

/// <summary>
/// @Cse19455
/// Real Player Controller<br></br>
/// <CLEANUP> @Cse19455
/// <MESSAGE-FOR-HYFABI> Do not touch my baby , otherwise i will kill u :)</MESSAGE>
/// Idc ~F
/// </summary>
//Client
public class Movement : MonoBehaviour {
	public static Movement Singleton { get; private set; }

	#region Properties + Atributes
	#region Player-Settings
	[SerializeField]
	private float movementSpeed;

	/// <summary>If creative mode with shift pressing => Move 3x as fast</summary>
	public float MovementSpeed {
		get => Input.GetKey(KeyCode.LeftShift) && PlayerVariables.Gamemode == Gamemode.CREATIVE ? movementSpeed * 3 : movementSpeed;
		set => movementSpeed = value;
	}

	public float JumpForce = 6f;
	public float fallMulti = 1.06f;
	private float camZoom = 20f;
	#endregion

	#region State-variables
	//TODO change to boolean lookinng left/right
	public bool LookingRight{ get => lookingRight; set {
			if(value == lookingRight)
				return;
			lookingRight = value;
			PlayerModelT.localScale = value ? Vector3.right : Vector3.left;
		}
	}
	private bool lookingRight;

	private bool lockvar = false;
	private float countdown;
	private float movement;
	#endregion

	public Rigidbody2D playerRigidbody;
	public ParticleSystem dust;
	public ParticleSystem wallDust;
	public int fallingDamageThreshhold; // how far can the player fall until he hurts himself
	public int startingHeight=-1;

	public bool CreativeMode => false; //Toogles Flying state

	private void CreateDust() => dust.Play();

	///public Animator animator;

	public Transform PlayerModelT => PlayerVariables.Singleton.playerModel.transform != null ? PlayerVariables.Singleton.playerModel.transform : throw new NullReferenceException();

	/// <summary>
	/// Player Position
	/// </summary>
	private Vector3 RigidBodyPosition
	{
		get => playerRigidbody.transform.position;
		set => playerRigidbody.transform.position = value;
	}

	#endregion

	/// <summary>Locking movement for direction</summary>
	public bool PlayerLocked
	{
		get => _playerLocked;
		set
		{
			playerRigidbody.simulated = !value;
			playerRigidbody.bodyType = value ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
			_playerLocked = value;//TODO: When player locked => Gamestate: LOADING
		}
	}
	private bool _playerLocked;

	#region UnityMethods
	public void Awake() => Singleton = this;

	/// <summary>
	/// Most frequent Update used for realtime movement calculations
	/// </summary>
	public void Update(){

		if (GameManager.State != GameState.INGAME)	return;

		Camera.main.orthographicSize = camZoom;
		if (movement > 0)
		{
			PlayerModelT.transform.rotation = new Quaternion(0, 0, 0, 0);
		}
		else if (movement < 0)
			PlayerModelT.transform.rotation = new Quaternion(0, 180, 0, 0);

		if (!PlayerLocked && PlayerVariables.Gamemode==Gamemode.CREATIVE)
			CreativeModeMovement();

		ExecuteCalculationsAndChecks();

		if (GameManager.State != GameState.INGAME)
			return;
		if (CheckChunk())
			return;
	}

	#endregion

	/// <summary>
	/// Executes all checks to Move the player correctly
	/// </summary>
	private void ExecuteCalculationsAndChecks()
    {
		VelocityUpdate();

		if (PlayerVariables.Gamemode != Gamemode.CREATIVE)
		{
			MoveHorizontally();
			FallAcceleration();
			PreventFloorGlitch();
		}
	}

	/// <summary> Let the player be locked if the chunk is not loaded/imported/visible</summary>
	/// <returns><see langword="true"/> if player is locked</returns>
	public bool CheckChunk()
	{
		//Dungeon
		if(PlayerVariables.Dimension == Dimension.DUNGEON){
			if(PlayerLocked)
				PlayerLocked = false;
			return false;
		}

		//Overworld
		bool locked = !ClientTerrainHandler.Singleton.CurrentChunkReady;
		if (locked != PlayerLocked)
			PlayerLocked = locked;
		return locked;
	}

	/// <summary>
	/// Creating walldust anim
	/// </summary>
	private void CreateWallDust()
	{
		wallDust.transform.position = new Vector3(GlobalVariables.LocalPlayerPos.x + wallDust.shape.position.x + 0.3f, GlobalVariables.LocalPlayerPos.y + wallDust.shape.position.y, GlobalVariables.LocalPlayerPos.z + wallDust.shape.position.z);
		wallDust.Play();
	}

	/// <summary>
	/// Updating Players Velocity (Vel checks => Moves like Wallkick - Walljump usw.)
	/// 
	/// <IMPORTANT>If you want to add unique movement style , pls do this here in this class , otherwise you could destroy the structural integrety of the Playermovement 
	/// </summary>
	public void VelocityUpdate()
	{
		if(PlayerVariables.Gamemode != Gamemode.CREATIVE)
			FallingDamageCalc();
		if (playerRigidbody.velocity.y != 0){
			if (Input.GetKeyDown(GameManager.SettingsProfile.JumpKey)||Input.GetKeyDown(KeyCode.Joystick1Button1))
			{
				if (TerrainHandler.Singleton.GetBlockFormCoordinate(
				WorldData.Singleton.Grid.WorldToCell(new Vector3(RigidBodyPosition.x + (-0.5f), RigidBodyPosition.y, 0)).x,
				WorldData.Singleton.Grid.WorldToCell(new Vector3(RigidBodyPosition.x, RigidBodyPosition.y - 2, 0)).y)
				!= 0 
				||
				TerrainHandler.Singleton.GetBlockFormCoordinate(
				WorldData.Singleton.Grid.WorldToCell(new Vector3(RigidBodyPosition.x + (0.5f), RigidBodyPosition.y, 0)).x,
				WorldData.Singleton.Grid.WorldToCell(new Vector3(RigidBodyPosition.x, RigidBodyPosition.y - 2, 0)).y)
				!= 0)
				{
					Walljump(); return;
				}
			}
			else if (lockvar && Input.GetKeyDown(GameManager.SettingsProfile.RollKey) || Input.GetKeyDown(KeyCode.Joystick1Button2))
			{
				Wallkick(); return;
			}
		}else
			if (Input.GetKey(GameManager.SettingsProfile.JumpKey) || Input.GetKeyDown(KeyCode.Joystick1Button1))
            {
				Jump();return;
            }
			//Roll
			else if (!lockvar && Input.GetKeyDown(GameManager.SettingsProfile.RollKey) || Input.GetKeyDown(KeyCode.Joystick1Button2))
            {
				Roll(); return;
			}
		SetHorizontalMovement();

	}

	/// <summary>
	/// Set Horizontal Movement
	/// </summary>
	private void SetHorizontalMovement(){
		if(!UIInventory.Singleton.ChatOpened && !UIInventory.Singleton.deathScreen.activeSelf && !PlayerLocked)
			movement = Input.GetAxis("Horizontal");
	}


	/// <summary>
	/// More or less a Long jump
	/// </summary>
	private void Roll()
	{
		playerRigidbody.AddRelativeForce(new Vector2(MovementSpeed * 1.2f * (LookingRight ? 1 : -1), JumpForce / 1.5f), ForceMode2D.Impulse);
	}

	/// <summary>
	/// Normal Jump on the Floor
	/// </summary>
	private void Jump()
	{
		//animator.SetBool("IsJumping", true);
		if (PlayerLocked)
			return;
		CreateDust();
		playerRigidbody.AddRelativeForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
	}

	/// <summary>
	/// Kick yourself away with more speed
	/// </summary>
	private void Wallkick()
	{
		CreateWallDust();
		playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0);
		playerRigidbody.AddRelativeForce(new Vector2((LookingRight ? 1 : -1) * -1 * 7, fallMulti * 1.5f), ForceMode2D.Impulse);
		countdown = 0.4f;
	}

	/// <summary>
	/// Normal Walljump
	/// </summary>
	private void Walljump()
	{
		CreateWallDust();
		playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0);
		playerRigidbody.AddRelativeForce(new Vector2((LookingRight ? 1 : -1) * -1 * 4, fallMulti * 4f), ForceMode2D.Impulse);
		countdown = 0.4f;
	}

	/// <summary>
	/// Movement for JESUS Mode
	/// </summary>
	private void CreativeModeMovement()
    {
		float x = Input.GetAxis("Horizontal");
		float y = Input.GetAxis("Vertical");
		Vector2 dir = new Vector2(x, y);
		playerRigidbody.velocity = (new Vector2(dir.x * MovementSpeed, dir.y * MovementSpeed));
	}

	/// <summary>
	/// Horizontal player Movement
	/// </summary>
	private void MoveHorizontally()
    {
		if (!lockvar ){
			float x = Input.GetAxis("Horizontal");

			if(PlayerVariables.Dimension == Dimension.OVERWORLD)
			{
				Vector3Int playerCell = WorldData.Singleton.Grid.WorldToCell(RigidBodyPosition);

				sbyte direction = 0;
				if (movement > 0)
					direction = 1;
				else if (movement < 0)
					direction = -1;

				if (!(TerrainHandler.Singleton.GetBlockFormCoordinate(playerCell.x+(int)direction, playerCell.y) != 0 
					|| TerrainHandler.Singleton.GetBlockFormCoordinate(playerCell.x + (int)direction, playerCell.y-1) != 0))
				{
					//Vector3 destination = RigidBodyPosition + MovementSpeed * new Vector3(movement, 0, 0);
					//playerRigidbody.velocity = Vector3.Lerp(RigidBodyPosition, destination, Time.deltaTime);

					
					playerRigidbody.velocity = (new Vector2(x * MovementSpeed, playerRigidbody.velocity.y));
				}
				return;
			}
			
			//Copied from above
			if(PlayerVariables.Dimension == Dimension.DUNGEON)
				playerRigidbody.velocity = (new Vector2(x * MovementSpeed, playerRigidbody.velocity.y));
		}
	}

	/// <summary>
	/// Calculate Players Falling Accelation of Player figure
	/// </summary>
	private void FallAcceleration()
    {
		if (playerRigidbody.velocity.y < 0)
			if (playerRigidbody.velocity.y > -15)
				RigidBodyPosition += Time.deltaTime * new Vector3(movement, (playerRigidbody.velocity.y) * fallMulti, 0);
	}

	/// <summary>
	/// Is just used when chunks are unable to load (Player doesn't stay in Wall)
	/// </summary>
	private void PreventFloorGlitch()
    {
		if(PlayerVariables.Dimension == Dimension.DUNGEON)
			return;
		Vector3Int playerCell = WorldData.Singleton.Grid.WorldToCell(RigidBodyPosition);
		if (TerrainHandler.Singleton.GetBlockFormCoordinate(playerCell.x, playerCell.y) != 0)
        {
			Debug.LogWarning("UWU - WALL_GLITCH_HANDLED");
			RigidBodyPosition = RigidBodyPosition + Vector3.up ;
        }
    }

	private void FallingDamageCalc()
    {
		
        if (playerRigidbody.velocity.y<0 && startingHeight==-1)
        {
			startingHeight = WorldData.Singleton.Grid.WorldToCell(RigidBodyPosition).y;
		}else if(playerRigidbody.velocity.y >= 0 && startingHeight!=-1)
        {
			int fallenHeight = startingHeight - WorldData.Singleton.Grid.WorldToCell(RigidBodyPosition).y;
            if (fallenHeight >= fallingDamageThreshhold)
            {
				float damage = (fallenHeight - fallingDamageThreshhold)*1.5f;
				PlayerHealth.Singleton.CurrentHealth = PlayerHealth.Singleton.CurrentHealth - (int)damage;
            }
			startingHeight = -1;
		}
    }

}
