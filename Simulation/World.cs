using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;
using Particles;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class World
{
	#pragma warning disable CS8602
	int width, height;
	public int resize_factor{get; private set;}
	public int worldTimer{get; private set;}=0;
	Chunk[,] sandbox;
    public RenderTexture2D canvas{get; private set;}

	int chunk_size=16;

	//Function to get random number(threadsafe)
	private static readonly Random random = new Random();
	private static readonly object syncLock = new object();
	public static int RandomNumber(int min, int max)
	{
		lock(syncLock) { // synchronize
			return random.Next(min, max);
		}
	}
	public static int RandomNumber(int max)
	{
		lock(syncLock) { // synchronize
			return random.Next(max);
		}
	}

	public static T RandomElement<T>(T[] array)
	{
		lock(syncLock) { // synchronize
			return array[random.Next(array.Length)];
		}
	}
	
	public World(int width, int height, int resize_factor)
	{
		chunk_size=width/20;
		canvas=LoadRenderTexture(width, height);
		this.width = width / resize_factor;
		this.height = height / resize_factor;
		this.resize_factor = resize_factor;		
		sandbox = new Chunk[width/chunk_size, height/chunk_size];
		
		
		for (int i = 0; i < width/chunk_size; i++)
		{
			for (int j = 0; j < height/chunk_size; j++)
			{
				sandbox[i, j] = new Chunk(chunk_size, i, j, this);
			}
		}
		//Create 40 rows of sand particles
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < 30; j++)
			{
				Set(i,j,new Sand(i,j,this));
			}
		}

	}

	public Particle Get(int x, int y)
	{
		if (x < 0 || x >= width || y < 0 || y >= height)
			return new Sand(x, y, this);
		
		Chunk chunk=GetChunkAt(x,y);
		//Get coordinates relative to chunk
		Vector2 chunkCoords=ToChunkCoordinates(x,y);

		return chunk.Get((int)chunkCoords.X,(int)chunkCoords.Y);
		
	}

	public Chunk GetChunkAt(int x, int y){
		//Get chunk from sandbox based on coordinates
		return sandbox[x/chunk_size,y/chunk_size];
	}
	
	public Vector2 ToChunkCoordinates(int x, int y){
		//Convert world coordinates to coordinates relative to chunk
		return new Vector2(x%chunk_size,y%chunk_size);
	}

	public void Move(int x, int y, int xto,int yto){
		if(xto<0||xto>=width||yto<0||yto>=height){
			return;
		}

		//Get chunk from sandbox based on destination 
		Chunk chunk_dest=GetChunkAt(xto,yto);
		Chunk chunk_src=GetChunkAt(x,y);

		Vector2 chunkCoords=ToChunkCoordinates(x,y);
		Vector2 chunkCoordsDest=ToChunkCoordinates(xto,yto);

		/*
		Particle may be in a different chunk at the destination coordinates. We need to make sure we
		swap chunks otherwise the coordinates will wrap and teleport it back to the opposite direction
		of which it is leaving. The draw back of having to do this is that there's now very visible seams between chunks
		as particles move between them.
		*/
		if(chunk_dest!=chunk_src){
			chunk_dest.Set((int)chunkCoordsDest.X,(int)chunkCoordsDest.Y,chunk_src.Get((int)chunkCoords.X,(int)chunkCoords.Y));
			chunk_src.Set((int)chunkCoords.X,(int)chunkCoords.Y,new Air((int)chunkCoords.X,(int)chunkCoords.Y,this));

			/*
			Not entirely sure why this happens, but if we don't return, we can end up with particles teleporting
			to the bottom of chunks. 
			*/
			return;
		}

		//Get coordinates relative to chun
		chunk_dest.Move((int)chunkCoords.X,(int)chunkCoords.Y,(int)chunkCoordsDest.X,(int)chunkCoordsDest.Y);
	}

	public void KeepAlive(int x, int y){
		Vector2 chunkCoords=ToChunkCoordinates(x,y);

		GetChunkAt(x,y).KeepAlive((int)chunkCoords.X,(int)chunkCoords.Y);
	}

	public void Set(int x, int y, Particle tile)
	{ 
		if (x < 0 || x >= width || y < 0 || y >= height)
			return;
		tile.timer=worldTimer+1;
		
		Chunk chunk=GetChunkAt(x,y);

		//Get coordinates relative to chunk
		Vector2 chunkCoords=ToChunkCoordinates(x,y);

		//Set the tile in the chunk
		chunk.Set((int)chunkCoords.X,(int)chunkCoords.Y,tile);
	}

	public void Paint(int x, int y, ParticleTypes particle, int brush_size)
	{
		if (x < 0 || x >= width || y < 0 || y >= height)
			return;


		for (int i = 0; i < brush_size; i++)
		{
			for (int j = 0; j < brush_size; j++)
			{
				switch(particle){
					case ParticleTypes.AIR:
						Set(x + i, y + j, new Air(x + i, y + j, this));
						break;
					case ParticleTypes.SAND:
						Set(x + i, y + j, new Sand(x + i, y + j, this));
						break;
					case ParticleTypes.WATER:
						Set(x + i, y + j, new Water(x + i, y + j, this));
						break;
				}
			}
		}
	}
	public bool InBounds(int x, int y)
	{
		return x >= 0 && x < width && y >= 0 && y < height;
	}
	public void Draw()
	{
		Rectangle source = new Rectangle(0, 0, canvas.texture.width, -canvas.texture.height);
        DrawTextureRec(canvas.texture, source, new Vector2(0, 0), Color.WHITE);

		//Debug draw chunks
		for (int i = 0; i < width/chunk_size; i++)
		{
			for (int j = 0; j < height/chunk_size; j++)
			{
				sandbox[i, j].DebugDraw();
			}
		}
	}

	public Vector2 toWorldCoorinates(Chunk originalChunk,int x, int y)
	{
		return new Vector2(
			x+(int)originalChunk.pos.X*chunk_size,
			y+(int)originalChunk.pos.Y*chunk_size
		);
	}
	public void Update()
	{
		worldTimer++;
		//Update chunks
		foreach(Chunk c in sandbox){
			if(c.sleep_timer>=c.sleep_time)
				continue;
			c.Update();
		}
		foreach(Chunk c in sandbox){
			if(c.sleep_timer>=c.sleep_time)
				continue;
			c.CommitCells();
		}		
		foreach(Chunk c in sandbox){
			if(c.sleep_timer>=c.sleep_time)
				continue;
			c.UpdateDirtRects();
		}
	}
}
