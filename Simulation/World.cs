using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;
using Particles;


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
		//Set(10,10,new Sand(10,10,this));
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
			Console.WriteLine("Out of bounds");
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
		of which it is leaving
		*/
		if(chunk_dest!=chunk_src){
			chunk_dest.Set((int)chunkCoordsDest.X,(int)chunkCoordsDest.Y,chunk_src.Get((int)chunkCoords.X,(int)chunkCoords.Y));
			chunk_src.Set((int)chunkCoords.X,(int)chunkCoords.Y,new Air((int)chunkCoords.X,(int)chunkCoords.Y,this));
			chunk_src.KeepAlive((int)chunkCoordsDest.X,(int)chunkCoordsDest.Y);
		}

		//Get coordinates relative to chunk
		
		chunk_dest.Move((int)chunkCoords.X,(int)chunkCoords.Y,(int)chunkCoordsDest.X,(int)chunkCoordsDest.Y);
	}

	public void KeepAlive(int x, int y){
		//Convert x,y to chunk coordinates
		Vector2 chunkCoords=ToChunkCoordinates(x,y);
		GetChunkAt(x,y).KeepAlive((int)chunkCoords.X,(int)chunkCoords.Y);
	}

	public void Set(int x, int y, Particle tile)
	{ 
		if (x < 0 || x >= width || y < 0 || y >= height){
			Console.WriteLine("Out of bounds");
			return;
		}
		tile.timer=worldTimer+1;
		
		Chunk chunk=GetChunkAt(x,y);
		//Get coordinates relative to chunk
		Vector2 chunkCoords=ToChunkCoordinates(x,y);
		//Set the tile in the chunk
		chunk.Set((int)chunkCoords.X,(int)chunkCoords.Y,tile);
	}

	public void Paint(int x, int y, ParticleTypes particle, int brush_size)
	{
		//Check if x, y is within bounds of sandbox array
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

	public void Draw()
	{
		Rectangle source = new Rectangle(0, 0, canvas.texture.width, -canvas.texture.height);
        DrawTextureRec(canvas.texture, source, new Vector2(0, 0), Color.WHITE);
		for (int i = 0; i < width/chunk_size; i++)
		{
			for (int j = 0; j < height/chunk_size; j++)
			{
				sandbox[i,j].DebugDraw();
			}
		}
	}

	public void Update()
	{
		worldTimer++;
		//Update all chunks
		for (int i = 0; i < width/chunk_size; i++)
		{
			for (int j = 0; j < height/chunk_size; j++)
			{
				sandbox[i,j].Update();
			}
		}
		for (int i = 0; i < width/chunk_size; i++)
		{
			for (int j = 0; j < height/chunk_size; j++)
			{
				sandbox[i,j].CommitCells();
			}
		}

		//Update the dirty rects of all chunks (doesn't seem to matter whether or not it goes before or after update)
		for (int i = 0; i < width/chunk_size; i++)
		{
			for (int j = 0; j < height/chunk_size; j++)
			{
				sandbox[i,j].UpdateDirtRects();
			}
		}
	}
}
