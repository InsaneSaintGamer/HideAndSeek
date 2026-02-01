using Sandbox;
using static StateOptions;
using static TeamOptions;

public sealed class BlackScreen : Component
{
	[Property] public GameObject Player { get; set; }
    [Property] public GameObject TeamManager { get; set; }
	public TeamOptions teamoptions;
	public StateOptions stateoptions;

	protected override void OnAwake()
	{
		teamoptions = TeamManager.GetComponent<TeamOptions>();
		stateoptions = Scene.Get<StateOptions>();

	}
	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		if ( Scene.Camera is null ) return;

		if ( stateoptions.CurrentState == GameState.PreRound && teamoptions.team == TeamOptions.Team.Seeker )
		{
			var hud = Scene.Camera.Hud;
			hud.DrawRect( new Rect( Vector2.Zero, Screen.Size ), Color.Black );
		}

	}
}
