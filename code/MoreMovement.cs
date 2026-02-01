using Sandbox;
using System;

public sealed class MoreMovement : Component
{
	[Sync( SyncFlags.FromHost )] public bool IsFrozen { get; set; }
	[Property] public float Stamina { get; set; } = 100f;
	[Property] public float MaxStamina { get; set; } = 100f;
	[Property] public float StaminaDrain { get; set; } = 10f;
	[Property] public float StaminaRegen { get; set; } = 20f;

	private TimeUntil _nextRegen;
	[Sync] public bool IsSprinting { get; set; } = false;

	[Property] public GameObject TeamManager { get; set; }
	public TeamOptions teamoptions;
	private PlayerController playerController;

	protected override void OnAwake()
	{
		teamoptions = TeamManager.GetComponent<TeamOptions>();
		playerController = Components.Get<PlayerController>();
	}

	protected override void OnUpdate()
	{
		if ( IsFrozen )
		{
			playerController.UseInputControls = false;
			playerController.WishVelocity = Vector3.Zero;
			return;
		}
		else
		{
			playerController.UseInputControls = true;
		}

		if ( Network.IsProxy ) return;

		IsSprinting = Input.Down( "Run" ) && Stamina > 0;

		if ( Stamina == 0 )
		{
			playerController.RunSpeed = playerController.WalkSpeed;
		}
		else
		{
			playerController.RunSpeed = teamoptions.team == TeamOptions.Team.Seeker ? 320 : 300;
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( Network.IsProxy ) return;

		if ( _nextRegen <= 0 && !IsSprinting )
		{
			StaminaRegeneration();
		}
		if ( IsSprinting )
		{
			Stamina -= StaminaDrain * Time.Delta;
			Stamina = Math.Clamp( Stamina, 0, MaxStamina );
			_nextRegen = 3f;
		}
	}
	void StaminaRegeneration()
	{
		Stamina += StaminaRegen * Time.Delta; ;
		Stamina = Math.Clamp( Stamina, 0, MaxStamina );
	}
}
