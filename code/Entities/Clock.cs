using Sandbox;
using System;
using Editor;
using System.Linq;
using System.Collections.Generic;
using Sandbox.Component;
using Sandbox.UI;

/// <summary>
/// Dings when entity interacts.
/// </summary>
[HammerEntity, SupportsSolid]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[Model]
[Title( "Clock" ), Category( "Lobby" ), Icon( "radio_button_checked" )]
public partial class ClockEntity : KeyframeEntity, IUse
{
	List<Entity> doors = new ();

	[Property( Title = "Activation distance" ), Category("Settings")]
	public int activationDist { get; set; } = 100;

	[Property( Title = "Door name" ), Category( "Settings" )]
	public string doorName { get; set; }

	public bool IsUsable( Entity user )
	{
		var pawn = user.Client.Pawn as MyPlayer;
		return user is Player && activationDist > Vector3.DistanceBetween( pawn.EyePosition, Position );
	}

	public bool OnUse( Entity user )
	{
		if ( !user.IsValid() ) return false;

		PlaySound( "bell_sound" );

		MyGame.AddPlayerReady( user );
		if ( MyGame.GetPlayersReady() < Game.Clients.Count ) return false;

		foreach ( DoorEntity entity in doors ) entity.Open();

		return false;
	}

	[Event.Entity.PostSpawn]
	public void AfterSpawn()
	{
		doors.AddRange( Entity.FindAllByName( doorName ) );
	}

	public override void Spawn()
	{
		base.Spawn();
		
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}

