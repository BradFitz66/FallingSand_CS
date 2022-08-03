using Raylib_cs;
using static Raylib_cs.Raylib;
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

	internal bool CheckRelative(int x, int y, string name)
	{
		return world.Get(x + this.x, y + this.y).name == name;
	}

	internal bool CheckDown(string type)
	{
		return world.Get(x, y + 1).name==type;
	}

	internal bool CheckUp(string type)
	{
		return world.Get(x, y - 1).name==type;
	}

	internal bool CheckRight(string type)
	{
		return world.Get(x + 1, y).name==type;
	}

	internal bool CheckLeft(string type)
	{
		return world.Get(x - 1, y).name==type;
	}

	internal bool CheckDownRight(string type)
	{
		return world.Get(x + 1, y + 1).name==type;
	}
	internal bool CheckDownLeft(string type)
	{
		return world.Get(x - 1, y + 1).name==type;
	}

	internal bool CheckUpRight(string type)
	{
		return world.Get(x + 1, y - 1).name==type;
	}

	internal bool CheckUpLeft(string type)
	{
		return world.Get(x - 1, y - 1).name==type;
	}

	public virtual Color GetColor()
	{
		return Color.BLACK;
	}
}

