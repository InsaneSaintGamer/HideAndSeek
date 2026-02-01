using Sandbox;
using System.Xml.Linq;
using static TeamOptions;

public sealed class ContactCollision : Component, Component.ICollisionListener
{
	[Property] public SoundEvent CaughtSoundEffect { get; set; }
	public StateOptions stateoptions;
	protected override void OnAwake()
	{
		stateoptions = Scene.Get<StateOptions>();
	}
	public void OnCollisionStart( Collision collision )
	{
		var self = collision.Self.GameObject.Root.Components.GetInChildren<TeamOptions>();
		var other = collision.Other.GameObject.Root.Components.GetInChildren<TeamOptions>();

		if ( self == null || other == null )
			return;

		if ( Networking.IsHost && stateoptions.CurrentState == StateOptions.GameState.ActiveRound && self.team == Team.Seeker && other.team == Team.Hider )
		{
			other.team = Team.Seeker;
			if ( self.Network.IsOwner )
			{
				Sound.Play( CaughtSoundEffect, self.GameObject.WorldPosition );
			}

			stateoptions.GetSeekerTotal();
			stateoptions.GetHiderTotal();

			if ( stateoptions.GetHiderTotal() == 0 )
				stateoptions.SeekerWin();
		}
	}
}
