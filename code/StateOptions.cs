using Sandbox;
using System;
using System.Threading.Tasks;
using static TeamOptions;

public sealed class StateOptions : Component, Component.INetworkListener
{
	[Property] public SoundEvent EndRoundSoundEffect { get; set; }
	[Sync][Property] public int HiderTotal { get; set; }
	[Sync][Property] public int SeekerTotal { get; set; }
	[Property] public GameObject Player { get; set; }
	public GameTimer gametimer;
	public TeamOptions teamoptions;

	protected override void OnAwake()
	{
		gametimer = Scene.Get<GameTimer>();
		teamoptions = Player.GetComponentInChildren<TeamOptions>();
		ChangeState( GameState.Waiting );
	}

	public enum WinningTeam
	{
		None,
		Hiders,
		Seekers
	}
	[Sync( SyncFlags.FromHost )][Property] public WinningTeam winningteam { get; set; }
	public enum GameState
	{
		Waiting,
		PreRound,
		ActiveRound,
		EndRound
	}

	[Sync( SyncFlags.FromHost )][Property] public GameState CurrentState { get; private set; }

	public void ChangeState( GameState newState )
	{
		if ( !Networking.IsHost )
			return;
		CurrentState = newState;

		switch ( CurrentState )
		{
			case GameState.Waiting:
				EnterWaiting();
				break;
			case GameState.PreRound:
				EnterPreRound();
				break;
			case GameState.ActiveRound:
				EnterActiveRound();
				break;
			case GameState.EndRound:
				EnterEndRound();
				break;
		}
	}

	private void EnterWaiting()
	{
		gametimer.RemainingTime = 0;
		if ( TotalPlayers() >= 2 )
		{
			ChangeState( GameState.PreRound );
		}

	}

	private void EnterPreRound()
	{
		gametimer.RemainingTime = 1;
		CreateAllHiders();
		CreateSeekers();
	}

	private void EnterActiveRound()
	{
		gametimer.RemainingTime = 5;
		FreezeSeekerToggle( false );
	}

	private void EnterEndRound()
	{
		if ( Networking.IsHost )
		{
			PlayEndRoundSound();
		}
	}

	public async Task RestartMatch()
	{
		await Task.DelaySeconds( 10 );

		ChangeState( GameState.Waiting );
		winningteam = WinningTeam.None;
		RespawnPlayers();
	}

	public async void HiderWin()
	{
		if ( winningteam == WinningTeam.None )
		{
			ChangeState( GameState.EndRound );
			winningteam = WinningTeam.Hiders;
			await RestartMatch();

		}
	}

	public async void SeekerWin()
	{
		if ( winningteam == WinningTeam.None )
		{
			ChangeState( GameState.EndRound );
			winningteam = WinningTeam.Seekers;
			await RestartMatch();

		}
	}




	public int TotalPlayers()
	{
		int TotalPlayers = Connection.All.Count();
		return TotalPlayers;
	}
	public int GetHiderTotal()
	{
		HiderTotal = Scene.GetAll<TeamOptions>().Where( x => x.team is Team.Hider ).Count();
		return HiderTotal;
	}
	public int GetSeekerTotal()
	{
		SeekerTotal = Scene.GetAll<TeamOptions>().Where( x => x.team is Team.Seeker ).Count();
		return SeekerTotal;
	}
	public void CreateSeekers()
	{
		var players = Scene.GetAll<TeamOptions>().ToList();
		var randomPlayer = players[new Random().Next( players.Count )];
		randomPlayer.team = Team.Seeker;
		FreezeSeekerToggle( true );
	}
	public void CreateAllHiders()
	{
		Scene.GetAll<TeamOptions>().ToList().ForEach( x => x.team = Team.Hider );
	}

	void Component.INetworkListener.OnDisconnected( Connection channel )
	{
		GetSeekerTotal();
		GetHiderTotal();

		if ( CurrentState == GameState.PreRound )
		{
			if ( TotalPlayers() < 1 )
			{
				CurrentState = GameState.Waiting;
			}
			else
			{
				CreateSeekers();
			}
		}
		else if ( CurrentState == GameState.ActiveRound )
		{
			if ( TotalPlayers() < 2 )
			{
				CurrentState = GameState.Waiting;
			}
			else if ( GetSeekerTotal() == 0 )
			{
				HiderWin();
			}
			else if ( GetHiderTotal() == 0 )
			{
				SeekerWin();
			}
		}
	}
	public void RespawnPlayers()
	{
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		foreach ( var players in Scene.GetAllComponents<PlayerController>().ToArray() )
		{
			if ( players.IsProxy )
				continue;

			var randomSpawnPoint = Random.Shared.FromArray( spawnPoints );

			players.WorldPosition = randomSpawnPoint.WorldPosition;
			players.EyeAngles = randomSpawnPoint.WorldRotation.Angles();

		}
	}
	[Rpc.Broadcast]
	public void PlayEndRoundSound()
	{
		Sound.Play( EndRoundSoundEffect, WorldPosition );
	}
	public void FreezeSeekerToggle( bool freeze )
	{
		foreach ( var user in Scene.GetAll<TeamOptions>() )
		{
			if ( user.team == Team.Seeker )
			{
				var movement = user.GetComponentInParent<MoreMovement>();
				if ( movement != null )
				{
					movement.IsFrozen = freeze;
				}
			}
		}
	}

}
