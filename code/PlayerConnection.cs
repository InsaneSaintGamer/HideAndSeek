using Sandbox;
using System.Xml.Linq;
using static TeamOptions;
using static StateOptions;

public sealed class PlayerConnection : Component, Component.INetworkSpawn
{
	public StateOptions stateoptions;
	[Property] public GameObject TeamManager { get; set; }
	public TeamOptions teamoptions;

	protected override void OnAwake()
	{
		teamoptions = TeamManager.Components.Get<TeamOptions>();
		stateoptions = Scene.Get<StateOptions>();
	}
	void Component.INetworkSpawn.OnNetworkSpawn( Connection channel )
	{

		if ( stateoptions.CurrentState == GameState.Waiting )
		{
			teamoptions.team = Team.Hider;
			if ( stateoptions.TotalPlayers() >= 2 )
			{
				stateoptions.ChangeState( GameState.PreRound );
			}
		}
		else if ( stateoptions.CurrentState == GameState.PreRound )
		{
			teamoptions.team = Team.Hider;
		}
		else if ( stateoptions.CurrentState == GameState.ActiveRound )
		{
			teamoptions.team = Team.Seeker;
		}
		else if ( stateoptions.CurrentState == GameState.EndRound )
		{
			teamoptions.team = Team.Seeker;
		}
		stateoptions.GetSeekerTotal();
		stateoptions.GetHiderTotal();
	}
}
