using Raylib_cs;
using static Raylib_cs.Raylib;
/*
Original author: Rudyon
Modified by: Badfitz67
Changes:
	Different name (kum was a bad name. Made it difficult if I wanted to use this project in any sort of professional capacity)
	Different rendering method for faster rendering (rendering to a render texture instead of drawing an individual rectangle for each particle)
	Chunking of the particles to improve performance
	Added a sleep timer to the particles to prevent them from updating when they cannot move
	Added a dirty rect to limit the amount of array elements we have to iterate over to update a chunk
*/
public enum ParticleTypes{
	SAND,
	AIR,
	WATER
}

public static class Extensions
{
    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] arr, int size)
    {
        for (var i = 0; i < arr.Length / size + 1; i++) {
            yield return arr.Skip(i * size).Take(size);
        }
    }
}

class Program
{
	public static void Main()
	{
		int width, height, resize_factor, mouse_x, mouse_y, brush_size;
		width = 600;
		height = 600;
		resize_factor = 4;
		brush_size = 4;
		ParticleTypes particle_type = ParticleTypes.SAND;
		Raylib.InitWindow(width, height, "Falling Sand");
		Raylib.SetTargetFPS(60);
		Raylib.HideCursor();

		World world = new World(width, height, resize_factor);



		while (!Raylib.WindowShouldClose())
		{
			BeginTextureMode(world.canvas);
			world.Update();

			mouse_x = Raylib.GetMouseX() / resize_factor;
			mouse_y = Raylib.GetMouseY() / resize_factor;

			if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
			{
				world.Paint(mouse_x, mouse_y,particle_type, brush_size);
			}

			else if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
			{
				//Cycle through the particle type enum

				//Get current index of particle_type in the enum
				int index = (int)particle_type;
				if(index+1>=Enum.GetNames(typeof(ParticleTypes)).Length)
				{
					particle_type = (ParticleTypes)0;
				}
				else
				{
					particle_type = (ParticleTypes)(index+1);
				}
			}

			if (Raylib.GetMouseWheelMove() != 0)
			{
				brush_size += (int)Raylib.GetMouseWheelMove();
			}

			if (brush_size < 1)
			{
				brush_size = 1;
			}
			EndTextureMode();
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.BLACK);
			world.Draw();
			Raylib.DrawText(particle_type.ToString(), 4, 4, 24, Color.WHITE);
			Raylib.DrawText("<space> to switch", 4, 32, 16, Color.WHITE);
			Raylib.DrawText("FPS: "+GetFPS().ToString(), 4, 48, 16, Color.WHITE);
			Raylib.DrawRectangleLines(mouse_x * resize_factor, mouse_y * resize_factor, brush_size * resize_factor, brush_size * resize_factor, Color.WHITE);

			Raylib.EndDrawing();
		}

		Raylib.CloseWindow();
	}
}
