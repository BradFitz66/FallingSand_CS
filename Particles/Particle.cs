using Raylib_cs;
using static Raylib_cs.Raylib;

public struct ParticleProperties{
	//Physical properties of the particle
	public float mass;
	public float density;
}

//Base class for all particles
public class Particle
{
	public int x, y;
	public World world;
	public int timer {get; set;}
	private Color color;
	public Chunk chunk;
	public float sleep_timer=0;
	public float sleep_time{get; private set;}=5;
	public string name {get; private set;}
	public ParticleProperties properties;

	public Particle(int x, int y, World world,string Name, Chunk c)
	{
		this.x = x;
		this.y = y;
		this.world = world;
		this.name=Name;
		this.chunk=c;
	}

	public virtual void Update()
	{
		sleep_timer+=1*GetFrameTime();
		
	}

	internal bool CheckRelative(int x, int y)
	{


		return world.Get(x + this.x, y + this.y).properties.density<properties.density;
	}

	internal bool CheckDown()
	{
		return world.Get(x, y + 1).properties.density<properties.density;
	}

	internal bool CheckUp()
	{
		return world.Get(x, y - 1).properties.density<properties.density;
	}

	internal bool CheckRight()
	{
		return world.Get(x + 1, y).properties.density<properties.density;
	}

	internal bool CheckLeft()
	{
		return world.Get(x - 1, y).properties.density<properties.density;
	}

	internal bool CheckDownRight()
	{
		return world.Get(x + 1, y + 1).properties.density<properties.density;
	}
	internal bool CheckDownLeft()
	{
		return world.Get(x - 1, y + 1).properties.density<properties.density;
	}

	internal bool CheckUpRight()
	{
		return world.Get(x + 1, y - 1).properties.density<properties.density;
	}

	internal bool CheckUpLeft()
	{
		return world.Get(x - 1, y - 1).properties.density<properties.density;
	}

	public virtual Color GetColor()
	{
		return Color.BLACK;
	}
}

