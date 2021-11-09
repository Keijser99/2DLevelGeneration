using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DrunkardWalkLevelGenerator : MonoBehaviour
{
    enum LevelTile {empty, floor, wall, bottomWall, start};
    LevelTile[,] grid; //Comma means it is a two dimensional array, so the variable 'grid' has two variables stored in the array
    struct RandomWalker 
	{
		public Vector2 dir;
		public Vector2 pos;
	}
    List<RandomWalker> walkers;

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
	public float chanceWalkerChangeDir = 0.5f;
    public int maxWalkers = 1;
    public int iterationSteps = 100000;

	private Vector2 playerSpawnpoint;
	private static DrunkardWalkLevelGenerator _instance;
	public static DrunkardWalkLevelGenerator Instance { get { return _instance; } }

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
		walkers = new List<RandomWalker>();
		RandomWalker walker = new RandomWalker();
		walker.dir = RandomDirection();

		Vector2 pos = new Vector2(Mathf.RoundToInt(levelWidth/ 2.0f), Mathf.RoundToInt(levelHeight/ 2.0f)); //Walker starts in the middle of the grid
		playerSpawnpoint = pos;
		walker.pos = pos;		

		grid[(int)pos.x, (int)pos.y] = LevelTile.start;

		walkers.Add(walker);
	}

	void CreateFloors() 
	{
		int iterations = 0;
		do{
			//assign floor enum member at position of every Walker
			foreach (RandomWalker walker in walkers)
			{
				grid[(int)walker.pos.x,(int)walker.pos.y] = LevelTile.floor;
			}

			//chance: Walker pick new direction
			for (int i = 0; i < walkers.Count; i++) 
			{
				if (Random.value < chanceWalkerChangeDir)
				{
					RandomWalker thisWalker = walkers[i];
					thisWalker.dir = RandomDirection();
					walkers[i] = thisWalker;
				}
			}

			//move Walkers
			for (int i = 0; i < walkers.Count; i++)
			{
				RandomWalker walker = walkers[i];
				walker.pos += walker.dir;
				walkers[i] = walker;				
			}

			//avoid boarder of grid
			for (int i =0; i < walkers.Count; i++)
			{
				RandomWalker walker = walkers[i];
				walker.pos.x = Mathf.Clamp(walker.pos.x, 1, levelWidth-2);
				walker.pos.y = Mathf.Clamp(walker.pos.y, 1, levelHeight-2);
				walkers[i] = walker;
			}

			//check to exit loop
			if ((float)NumberOfFloors() / (float)grid.Length > percentToFill)
			{
				break;
			}
			iterations++;
		} while(iterations < iterationSteps);
	}

	//Replaces the empty tiles around the floor tiles with wall tiles
	void CreateWalls(){
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

	Vector2 RandomDirection()
	{   //Chooses a random direction (north, east, south, west)
		int choice = Mathf.FloorToInt(Random.value * 3.99f);
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
		foreach (LevelTile space in grid){
			if (space == LevelTile.floor){
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

	//Places the player in the middle of the grid
	void SpawnPlayer() 
	{
		Vector3 playerPos = new Vector3(Mathf.RoundToInt(levelWidth / 2.0f), Mathf.RoundToInt(levelHeight / 2.0f), 0);
		GameObject playerObj = Instantiate(player, playerPos, Quaternion.identity) as GameObject;
    }

    public void SpawnExit() 
	{

        Vector2 playerPos = new Vector2(Mathf.RoundToInt(levelWidth/ 2.0f), Mathf.RoundToInt(levelHeight/ 2.0f));
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

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(playerSpawnpoint, .5f);
	}
}