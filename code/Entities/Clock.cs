using Sandbox;
using System;
using Editor;
using System.Linq;
using System.Collections.Generic;
using Sandbox.Component;

/// <summary>
/// Dings when entity interacts.
/// </summary>
[HammerEntity, SupportsSolid]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[Model]
[Title( "Clock" ), Category( "Placeable" ), Icon( "radio_button_checked" )]
public partial class ClockEntity : KeyframeEntity, IUse
{
	List<Entity> doors = new ();

	[Property( Title = "Activation distance" ), Category("Settings")]
	public int activationDist { get; set; } = 100;

	[Property( Title = "Door name" ), Category( "Settings" )]
	public string doorName { get; set; }

	public bool IsUsable( Entity user )
	{
		return user is Player && activationDist > Vector3.DistanceBetween(user.Position, Position);
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

		Components.Add( new Glow() );

		var glow = Components.Get<Glow>();
		glow.Color = Color.White;
		glow.Width = 0.5f;
		glow.Enabled = false;
	}

	public override void Spawn()
	{
		base.Spawn();

		//Brug måske SetAnimParameter(Name, Value)
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}

