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

	public Vector3 secondSpawn = new Vector3( 204, 203, 1682 );
	//x 240 y 203 z 1682

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

		var rand = new Random();
		var playerIndex = rand.Next( 0, 2 );

		var pawn = Game.Clients.ToList()[playerIndex].Pawn as MyPlayer;

		pawn.Position = secondSpawn;

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

