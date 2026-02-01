using Sandbox;
using System;
using static StateOptions;

public sealed class GameTimer : Component
{
	[Sync( SyncFlags.FromHost )][Property] public float RemainingTime { get; set; }
	[Property] public int Minutes { get; set; }
	[Property] public int Seconds { get; set; }

	public StateOptions stateoptions;

	protected override void OnAwake()
	{
		stateoptions = Scene.Get<StateOptions>();
	}


	protected override void OnUpdate()
	{
		if ( !Networking.IsHost )
			return;
		if ( RemainingTime > 0 )
		{
			RemainingTime -= Time.Delta;
		}
		else if ( RemainingTime <= 0 )
		{
			RemainingTime = 0;

			if ( stateoptions.CurrentState == GameState.PreRound )
			{
				stateoptions.ChangeState( GameState.ActiveRound );
			}
			else if ( stateoptions.CurrentState == GameState.ActiveRound )
			{
				stateoptions.HiderWin();
			}

		}
		Minutes = (int)MathF.Floor( RemainingTime / 60 );
		Seconds = (int)MathF.Floor( RemainingTime % 60 );
	}

}

