using UnityEngine;

public class RebuildedHuntAndKillLevelGenerator : MonoBehaviour
{
	/*
    1. Choose a starting location.
    2. Perform a random walk, carving passages to unvisited neighbors, until the current cell has no unvisited neighbors.
    3. Enter “hunt” mode, where you scan the grid looking for an unvisited cell that is adjacent to a visited cell. if found, carve a passage between the two and let the formerly unvisited cell be the new starting location.
    4. Repeat steps 2 and 3 until the hunt mode scans the entire grid and finds no unvisited cells.
	*/

	enum LevelTile {empty, floor, wall, bottomWall, start};
    LevelTile[,] grid; //Comma means it is a two dimensional array, so the variable 'grid' has two variables stored in the array
    struct RandomWalker 
	{
		public Vector2 dir;
		public Vector2 pos;
	}

    public GameObject[] floorTiles; //Floors are generated first by the Drunkards Walk Algorithm
    public GameObject[] wallTiles; //Generates around placed floor tiles
    public GameObject[] bottomWallTiles; //Overwrites generated wall tiles that have a floor tile under them
    public GameObject exit;
    public GameObject player;
	public GameObject start;

	[SerializeField]
	[Range(1, 50)]
	private int levelWidth;

	[SerializeField]
	[Range(1, 50)]
	private int levelHeight;

	public float percentToFill = 0.2f;
    public int iterationSteps = 100000;

	private Vector2 playerSpawnpoint;
	private Vector2 currentWalkerPosition;

	bool northEmpty = false, eastEmpty = false, southEmpty = false, westEmpty = false;

	RandomWalker theWalker;
	Vector2 walkerPos, minedWallPos;

	public bool canDebug;

	private static RebuildedHuntAndKillLevelGenerator _instance;
	public static RebuildedHuntAndKillLevelGenerator Instance { get { return _instance; } }

	void Awake()
	{
		//Makes this script a singleton, so that it is easier accessible and makes sure that there is only one of it at the time
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			_instance = this;
		}

		Setup();
        CreateFloors();    
        CreateWalls();
        CreateBottomWalls();
        SpawnLevel();
		SpawnStartTile();
        SpawnPlayer();
        SpawnExit();
    }

    void Setup()
	{
        // prepare grid
		grid = new LevelTile[levelWidth, levelHeight];

		//fill 'grid' with empty Level tiles
		for (int x = 0; x < levelWidth - 1; x++)
		{
			for (int y = 0; y < levelHeight - 1; y++)
			{ 
				grid[x, y] = LevelTile.empty;
			}
		}

        //generate first walker
		theWalker = new RandomWalker();
		theWalker.dir = RandomDirection();

		walkerPos = new Vector2(Mathf.RoundToInt(Random.Range(1, levelWidth - 1)), Mathf.RoundToInt(Random.Range(1, levelHeight - 1))); //Walker starts on a random location
		playerSpawnpoint = walkerPos;
		theWalker.pos = walkerPos;

		grid[(int)walkerPos.x, (int)walkerPos.y] = LevelTile.start;
		currentWalkerPosition = theWalker.pos;
	}

	void CreateFloors() 
	{
		int iterations = 0;
		do{
			//Every tile the walker visits will become a floor tile
			grid[(int)theWalker.pos.x,(int)theWalker.pos.y] = LevelTile.floor;

			//Set direction of position of walker
			theWalker.dir = NewDirection();
			theWalker.pos += theWalker.dir;				

			//Avoid the borders of the grid
			theWalker.pos.x = Mathf.Clamp(theWalker.pos.x, 1, levelWidth-2);
			theWalker.pos.y = Mathf.Clamp(theWalker.pos.y, 1, levelHeight-2);

			//Check to exit loop
			if ((float)NumberOfFloors() / (float)grid.Length > percentToFill)
			{
				break;
			}
			iterations++;
		} while(iterations < iterationSteps);
	}

	//Replaces the empty tiles around the floor tiles with wall tiles
	void CreateWalls()
	{
		for (int x = 0; x < levelWidth-1; x++) 
		{
			for (int y = 0; y < levelHeight-1; y++) 
			{
				if (grid[x,y] == LevelTile.floor) 
				{
					if (grid[x,y+1] == LevelTile.empty) 
					{
						grid[x,y+1] = LevelTile.wall;
					}

					if (grid[x,y-1] == LevelTile.empty) 
					{
						grid[x,y-1] = LevelTile.wall;
					}
					if (grid[x+1,y] == LevelTile.empty) 
					{
						grid[x+1,y] = LevelTile.wall;
					}
					if (grid[x-1,y] == LevelTile.empty) 
					{
						grid[x-1,y] = LevelTile.wall;
					}

                    if (grid[x - 1, y - 1] == LevelTile.empty) 
					{
                        grid[x - 1, y - 1] = LevelTile.wall;
                    }
                    if (grid[x - 1, y + 1] == LevelTile.empty) 
					{
                        grid[x - 1, y + 1] = LevelTile.wall;
                    }
                    if (grid[x + 1, y + 1] == LevelTile.empty) 
					{
                        grid[x + 1, y + 1] = LevelTile.wall;
                    }
                    if (grid[x + 1, y - 1] == LevelTile.empty) 
					{
                        grid[x + 1, y - 1] = LevelTile.wall;
                    }
                }
            }
		}
	}

	//Replaces walls that have a floor under it with a wall that has a 3D effect
	void CreateBottomWalls()
	{
		for (int x = 0; x < levelWidth; x++) 
		{
			for (int y = 1; y < levelHeight; y++) 
			{
				if (grid[x, y] == LevelTile.wall && grid[x, y - 1] == LevelTile.floor) 
				{
                    grid[x, y] = LevelTile.bottomWall;
                }
            }
		}
	}

	void SpawnLevel()
	{
		for (int x = 0; x < levelWidth; x++)
		{
			for (int y = 0; y < levelHeight; y++)
			{
				//spawn the correct sprites for the tiles from the respective list
				switch (grid[x, y])
				{
					case LevelTile.empty:
						break;
					case LevelTile.floor:
						Spawn(x, y, floorTiles[Random.Range(0, floorTiles.Length)]);
						break;
					case LevelTile.start:
						Spawn(x, y, start);
						break;
					case LevelTile.wall:
						Spawn(x, y, wallTiles[Random.Range(0, wallTiles.Length)]);
						break;
					case LevelTile.bottomWall:
						Spawn(x, y, bottomWallTiles[Random.Range(0, bottomWallTiles.Length)]);
						break;
				}
			}
		}
	}

	//Decides which new direction the walker needs to make. It can't go on a point in the grid it already has been to
	private Vector2 NewDirection()
    {
		int choice = Random.Range(0, 4); //Chooses a random direction (north = 0, east = 1, south = 2, west = 3)
		if(canDebug) Debug.Log("choice =" + choice);

		int x = Mathf.FloorToInt(currentWalkerPosition.x);
		int y = Mathf.FloorToInt(currentWalkerPosition.y);

		northEmpty = false;
		eastEmpty = false;
		southEmpty = false;
		westEmpty = false;

		//current location of the walker on the grid
		if(canDebug) Debug.Log($"Where is the walker currently standing on? -> {grid[x, y]}");
		if (grid[x, y] == LevelTile.floor || grid[x, y] == LevelTile.start)
		{
			if (grid[x, y + 1] == LevelTile.empty) //empty above given floor
			{
				northEmpty = true;
			}

			if (grid[x, y - 1] == LevelTile.empty) //empty under given floor
			{
				southEmpty = true;
			}

			if (grid[x + 1, y] == LevelTile.empty) //empty right of given floor
			{
				eastEmpty = true;
			}

			if (grid[x - 1, y] == LevelTile.empty) //empty left of given floor
			{
				westEmpty = true;
			}
		}

		//Checks if the chosen direction already is a floor
        if (northEmpty == true && choice == 0)
        {
			if(canDebug) Debug.Log("Going UP");
			return Vector2.up;
        }
        else
        {
			northEmpty = false;
        }
		if (eastEmpty == true && choice == 1)
		{
			if(canDebug) Debug.Log("Going RIGHT");
			return Vector2.right;
		}
        else
        {
			eastEmpty = false;
        }
		if (southEmpty == true && choice == 2)
		{
			if(canDebug) Debug.Log("Going DOWN");
			return Vector2.down;
		}
        else
        {
			southEmpty = false;
        }
		if (westEmpty == true && choice == 3)
		{
			if(canDebug) Debug.Log("Going LEFT");
			return Vector2.left;
		}
        else
        {
			westEmpty = false;
        }

		if (northEmpty == false && eastEmpty == false && southEmpty == false && westEmpty == false)
        {
			if(canDebug) Debug.Log("I am stuck. Looking for new position...");
			theWalker.pos = HuntForNewPosition();
			if(canDebug) Debug.Log("Hunted position = " + HuntForNewPosition());
			return Vector2.zero;
        }
        else
        {
			if(canDebug) Debug.Log("This direction is OBSTRUCTED. Trying again...");
			NewDirection();
			return Vector2.zero;
        }
	}

	Vector2 HuntForNewPosition()
    {
		Vector2 newPos = new Vector2(0,0);
		

		for (int x = 0; x < levelWidth - 1; x++)
		{
			for (int y = 0; y < levelHeight - 1; y++)
			{
				if (grid[x, y] == LevelTile.empty)
				{
					newPos = new Vector2(x, y);
					if(canDebug) Debug.Log($"Position of New Walker: {theWalker.pos}, new Position: {newPos}");
					return newPos;
				}
			}
		}
		return newPos;
	}

	Vector2 RandomDirection()
	{   //Chooses a random direction (north, east, south, west)
		int choice = Mathf.FloorToInt(Random.value * 3.99f);
		if(canDebug) Debug.Log($"First choice of direction = {choice}");
		switch (choice)
		{
			case 0:
				return Vector2.down;
			case 1:
				return Vector2.left;
			case 2:
				return Vector2.up;
			default:
				return Vector2.right;
		}
	}

	int NumberOfFloors()
	{
		int count = 0;
		foreach (LevelTile space in grid)
		{
			if (space == LevelTile.floor)
			{
				count++;
			}
		}
		return count;
	}

	//Places start tile on the first point of the walker
	void SpawnStartTile()
	{
		Vector2 startTile = playerSpawnpoint;
		Spawn(startTile.x, startTile.y, start);
	}

	//Places the player on the first point of the walker
	void SpawnPlayer()
	{
		Vector3 playerPos = playerSpawnpoint;
		GameObject playerObj = Instantiate(player, playerPos, Quaternion.identity) as GameObject;
    }

    public void SpawnExit()
	{
		Vector2 playerPos = walkerPos;
        Vector2 exitPos = playerPos;
        float exitDistance = 0f;

		//This will place the exit gate on a floor tile the furthest possible from the starting point as possible
		for (int x = 0; x < levelWidth - 1; x++)
		{
			for (int y = 0; y < levelHeight - 1; y++)
			{
				if (grid[x,y] == LevelTile.floor)
				{
                    Vector2 nextPos = new Vector2(x, y);
                    float distance = Vector2.Distance(playerPos, nextPos);
                    if (distance > exitDistance) 
					{
                        exitDistance = distance;
                        exitPos = nextPos;
                    }
                }
            }
        }

        Spawn(exitPos.x, exitPos.y, exit);
    }       

	void Spawn(float x, float y, GameObject toSpawn) 
	{
		Instantiate(toSpawn, new Vector3(x, y, 0), Quaternion.identity);
	}

	//Gets the location of the tile that is mined and places a floor there
	public void MiningWalls(GameObject minedWall)
    {
		minedWallPos = minedWall.transform.position;

		minedWallPos.x = Mathf.Clamp(minedWallPos.x, 1, levelWidth - 2);
		minedWallPos.y = Mathf.Clamp(minedWallPos.y, 1, levelHeight - 2);

		grid[(int)minedWallPos.x, (int)minedWallPos.y] = LevelTile.floor;

		Spawn(minedWallPos.x, minedWallPos.y, floorTiles[Random.Range(0, floorTiles.Length)]);

		CheckForNewWallPositions();
	}

	private void CheckForNewWallPositions()
    {
		//Check around the the position of last mined wall to see whether there is an empty space next to the new floor
		if (grid[(int)minedWallPos.x, (int)minedWallPos.y + 1] == LevelTile.empty)
		{
			grid[(int)minedWallPos.x, (int)minedWallPos.y + 1] = LevelTile.wall;
			Spawn(minedWallPos.x, minedWallPos.y + 1, wallTiles[Random.Range(0, wallTiles.Length)]);
		}

		if (grid[(int)minedWallPos.x, (int)minedWallPos.y - 1] == LevelTile.empty)
		{
			grid[(int)minedWallPos.x, (int)minedWallPos.y - 1] = LevelTile.wall;
			Spawn(minedWallPos.x, minedWallPos.y - 1, wallTiles[Random.Range(0, wallTiles.Length)]);
		}

		if (grid[(int)minedWallPos.x + 1, (int)minedWallPos.y] == LevelTile.empty)
		{
			grid[(int)minedWallPos.x + 1, (int)minedWallPos.y] = LevelTile.wall;
			Spawn(minedWallPos.x + 1, minedWallPos.y, wallTiles[Random.Range(0, wallTiles.Length)]);
		}

		if (grid[(int)minedWallPos.x - 1, (int)minedWallPos.y] == LevelTile.empty)
		{
			grid[(int)minedWallPos.x - 1, (int)minedWallPos.y] = LevelTile.wall;
			Spawn(minedWallPos.x - 1, minedWallPos.y, wallTiles[Random.Range(0, wallTiles.Length)]);
		}


		if (grid[(int)minedWallPos.x - 1, (int)minedWallPos.y - 1] == LevelTile.empty)
		{
			grid[(int)minedWallPos.x - 1, (int)minedWallPos.y - 1] = LevelTile.wall;
			Spawn(minedWallPos.x - 1, minedWallPos.y - 1, wallTiles[Random.Range(0, wallTiles.Length)]);
		}

		if (grid[(int)minedWallPos.x - 1, (int)minedWallPos.y + 1] == LevelTile.empty)
		{
			grid[(int)minedWallPos.x - 1, (int)minedWallPos.y + 1] = LevelTile.wall;
			Spawn(minedWallPos.x - 1, minedWallPos.y + 1, wallTiles[Random.Range(0, wallTiles.Length)]);
		}

		if (grid[(int)minedWallPos.x + 1, (int)minedWallPos.y + 1] == LevelTile.empty)
		{
			grid[(int)minedWallPos.x + 1, (int)minedWallPos.y + 1] = LevelTile.wall;
			Spawn(minedWallPos.x + 1, minedWallPos.y + 1, wallTiles[Random.Range(0, wallTiles.Length)]);
		}

		if (grid[(int)minedWallPos.x + 1, (int)minedWallPos.y - 1] == LevelTile.empty)
		{
			grid[(int)minedWallPos.x + 1, (int)minedWallPos.y - 1] = LevelTile.wall;
			Spawn(minedWallPos.x + 1, minedWallPos.y - 1, wallTiles[Random.Range(0, wallTiles.Length)]);
		}


		//Also spawn bottom walls on wall locations where there is a floor tile under it
		for (int x = 0; x < levelWidth; x++)
		{
			for (int y = 1; y < levelHeight; y++)
			{
				if (grid[x, y] == LevelTile.wall && grid[x, y - 1] == LevelTile.floor)
				{
					grid[x, y] = LevelTile.bottomWall;
					Spawn(x, y, bottomWallTiles[Random.Range(0, bottomWallTiles.Length)]);
				}
			}
		}
	}
}