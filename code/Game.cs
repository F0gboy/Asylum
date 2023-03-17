using Sandbox;
using Sandbox.UI;
using System.Linq;
using System.Threading.Tasks;
using static Sandbox.Event;
using static Sandbox.StreamPoll;

partial class MyGame : GameManager
{
	MyWorldPanel myWorldPanel;

	public MyGame()
	{
		if ( Game.IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var player = new MyPlayer( client);
		client.Pawn = player;

		player.Respawn();

	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	static async Task<string> SpawnPackageModel( string packageName, Vector3 pos, Rotation rotation, Sandbox.Entity source )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package == null || package.PackageType != Package.Type.Model || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		if ( !source.IsValid ) return null; // source entity died or disconnected or something

		var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );
		var mins = package.GetMeta( "RenderMins", Vector3.Zero );
		var maxs = package.GetMeta( "RenderMaxs", Vector3.Zero );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		return model;
	}

	static bool CanSpawnPackage( Package package )
	{
		if ( package.PackageType != Package.Type.Addon ) return false;
		if ( !package.Tags.Contains( "runtime" ) ) return false;

		return true;
	}

	[ClientRpc]
	internal static void RespawnEntitiesClient()
	{
		Sandbox.Game.ResetMap( All.Where( x => !DefaultCleanupFilter( x ) ).ToArray() );
	}

	static bool DefaultCleanupFilter( Sandbox.Entity ent )
	{
		// Basic Source engine stuff
		var className = ent.ClassName;
		if ( className == "player" || className == "worldent" || className == "worldspawn" || className == "soundent" || className == "player_manager" )
		{
			return false;
		}

		// When creating entities we only have classNames to work with..
		// The filtered entities below are created through code at runtime, so we don't want to be deleting them
		if ( ent == null || !ent.IsValid ) return true;

		// Gamemode entity
		if ( ent is BaseGameManager ) return false;

		// HUD entities
		if ( ent.GetType().IsBasedOnGenericType( typeof( HudEntity<> ) ) ) return false;

		// Player related stuff, clothing and weapons
		foreach ( var cl in Game.Clients )
		{
			if ( ent.Root == cl.Pawn ) return false;
		}

		// Do not delete view model
		if ( ent is BaseViewModel ) return false;

		return true;
	}
}
