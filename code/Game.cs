using Sandbox;
using Sandbox.Entities;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

partial class MyGame : GameManager
{
	MyWorldPanel myWorldPanel;
	
	private static List<Sandbox.Entity> playersReady = new ();
	private bool gameStarted = false;
	private static bool allPlayersReady = false;

	private static List<Entity> papersCollected = new();
	public static List<Key> keysCollected { get; private set; } = new();
	public static PaperManager papers { get; private set; }
	public static Keypad keypad { get; private set; }
	public static Countdown countdown { get; private set; }
	private static PlayersReady playersReadyUi;

	List<SpotLightEntity> lights = new();
	Random rand = new();

	public MyGame()
	{
		if ( Game.IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();
		}

		if ( Game.IsClient )
		{
			_ = new Interactebels();
			_ = new Notifications( "You picked up a key.");
			countdown = new Countdown();
			
			_ = new PlayersReady();
			UpdateReadyUi(playersReady.Count, Game.Clients.Count);
		}
	}

	public static List<Entity> GetPapersCollected(  )
	{
		return papersCollected;
	}

	public static void AddPaperCollected( Entity paper )
	{
		if ( papersCollected.Contains( paper ) ) return;

		papersCollected.Add( paper );

		foreach ( var client in Game.Clients )
		{
			CreateNotification( To.Single( (client.Pawn as MyPlayer)),
				$"New paper collected: {papersCollected.Count}/6" );
		}
	}

	[ClientRpc]
	public static void CreateNotification( string text )
	{
		_ = new Notifications( text );
	}

	[ClientRpc]
	public static void UpdateReadyUi(int readyCount, int playerIngame)
	{
		Log.Info( "Hej" );
		Event.Run( "UpdateReady", readyCount, playerIngame );
	}

	[Event( "KeyCollected" )]
	public void KeyCollected( Key keyCollected )
	{
		keysCollected.Add( keyCollected );
		Log.Info( "Key collected" );
	}

	public static void SetPapers(PaperManager paperManager)
	{
		if ( papers.IsValid() ) return;
		papers = paperManager;
	}

	public static void SetKeypad( Keypad pad )
	{
		if ( keypad.IsValid() ) return;
		keypad = pad;
	}

	public static void OpenOfficeDoor()
	{
		CombinationLock.OpenDoor();
	}

	public static void AddPlayerReady( Sandbox.Entity client )
	{
		if ( playersReady.Contains( client ) ) return;

		playersReady.Add( client );

		if ( GetPlayersReady() == Game.Clients.Count ) allPlayersReady = true;

		foreach (var loopClient in Game.Clients)
		{
			UpdateReadyUi( To.Single( loopClient.Pawn as MyPlayer ), playersReady.Count, Game.Clients.Count );
		}
	}

	public static int GetPlayersReady()
	{
		return playersReady.Count;
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var player = new MyPlayer( client );
		client.Pawn = player;
		
		player.Respawn();

		foreach ( var loopClient in Game.Clients )
		{
			UpdateReadyUi( To.Single( loopClient.Pawn as MyPlayer ), playersReady.Count, Game.Clients.Count );
		}
	}

	public float startIntensity = 1;
	public float minIntensity = 0.3f;
	public float maxIntensity = 1f;
	public int smoothing = 5;
	public bool fullFlicker = false;
	public float fullFlickerBias = 0;
	Queue<float> smoothQueue;
	float lastSum = 0;

	[Event.Entity.PostSpawn]
	public void OnPostSpawn()
	{
		smoothQueue = new Queue<float>( smoothing );
		foreach (var ent in SpotLightEntity.All)
		{
			if ( ent is SpotLightEntity && ent.Name == "Flicker" )
			{
				SwitchFlickLight( (SpotLightEntity)ent );
				//lights.Add( (SpotLightEntity)ent );
			}
		}
	}

	//[Event.Tick.Server]
	//public void OnTick()
	//{
	//	foreach ( SpotLightEntity ent in lights )
	//		FlickLight( ent );
	//}

	public async void SwitchFlickLight( SpotLightEntity spot )
	{
		Log.Info(spot.Brightness);
		var onBrightness = spot.Brightness;
		var wait = 0;

		while ( true )
		{
			for ( float i = 1; i > 0; i -= .1f)
			{
				spot.Brightness = i;
				await GameTask.Delay( 15 );
			}

			spot.Brightness = 0;

			wait = rand.Next( 250, 1000 );
			await GameTask.Delay( wait );
			
			spot.Brightness = onBrightness;

			for ( float i = 0; i < 1; i += .1f )
			{
				spot.Brightness = i;
				await GameTask.Delay( 15 );
			}

			spot.Brightness = 1;

			wait = rand.Next( 250, 1000 );

			for ( var i = 0; i < wait; i += 1000 / 60 )
			{
				await GameTask.Delay( 1000 / 60 );

				FlickLight( spot );
			}

		}
	}

	public void FlickLight(SpotLightEntity spot )
	{
		maxIntensity = startIntensity;

		spot.Brightness = startIntensity;

		// pop off an item if too big
		while ( smoothQueue.Count >= smoothing )
		{
			lastSum -= smoothQueue.Dequeue();
		}

		// Generate random new item, calculate new average
		float newVal = rand.Float( minIntensity, maxIntensity );
		smoothQueue.Enqueue( newVal );
		lastSum += newVal;

		float newIntensity = lastSum / (float)smoothQueue.Count;


		spot.Brightness = newIntensity;

		if ( fullFlicker )
		{
			if ( spot.Brightness > ((maxIntensity - minIntensity) / 2) + (-fullFlickerBias * (maxIntensity - minIntensity) / 2) )
			{
				spot.Brightness = maxIntensity;
			}
			else
			{
				spot.Brightness = minIntensity;
			}
		}
	}

	//[Event.Tick.Server]
	//public void Tick()
	//{
	//	foreach ( SpotLightEntity light in lights )
	//	{
	//		light.Brightness = rand.Float( 1f, 0.5f );
	//	}
	//}

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

	public static float MapRange( float x, float inMin, float inMax, float outMin, float outMax )
	{
		// Map the input range to the output range
		var mappedValue = (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

		// Clamp the mapped value to the output range
		// Clamp the mapped value to the output range
		if ( mappedValue < outMin )
		{
			mappedValue = outMin;
		}
		else if ( mappedValue > outMax )
		{
			mappedValue = outMax;
		}

		return mappedValue;
	}

	[ClientRpc]
	internal static void RespawnEntitiesClient()
	{
		Game.ResetMap( All.Where( x => !DefaultCleanupFilter( x ) ).ToArray() );
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
