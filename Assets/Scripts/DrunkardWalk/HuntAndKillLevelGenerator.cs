using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class HuntAndKillLevelGenerator : MonoBehaviour
{
	/*
    1. Choose a starting location.
    2. Perform a random walk, carving passages to unvisited neighbors, until the current cell has no unvisited neighbors.
    3. Enter “hunt” mode, where you scan the grid looking for an unvisited cell that is adjacent to a visited cell. If found, carve a passage between the two and let the formerly unvisited cell be the new starting location.
    4. Repeat steps 2 and 3 until the hunt mode scans the entire grid and finds no unvisited cells.
	*/

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
	[Range (1, 50)]
    private int levelWidth;

	[SerializeField]
	[Range(1, 50)]
	private int levelHeight;

	public float percentToFill; 
	public float chanceWalkerChangeDir;
    public float chanceWalkerSpawn;
    public float chanceWalkerDestoy;
    public int maxWalkers;
    public int iterationSteps;

	private Vector2 playerSpawnpoint;
	private Vector2 currentWalkerPosition;

	RandomWalker walker;


	void Awake()
	{
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
        //prepare grid
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
		walker = new RandomWalker();

		Vector2 walkerPos = new Vector2(Mathf.RoundToInt(Random.Range(1, levelWidth -1)), Mathf.RoundToInt(Random.Range(1, levelHeight -1))); //Walker starts on a random location
		Debug.Log("First walker spawned on: " + walkerPos);

		grid[(int)walkerPos.x, (int)walkerPos.y] = LevelTile.start;

		currentWalkerPosition = walkerPos;

		walker.dir = Vector2.right;
		walker.pos = walkerPos;

		playerSpawnpoint = walkerPos;

		walkers.Add(walker);
		walker.dir = RandomDirection();
	}

	private void AddNewWalker()
    {
		//Debug.Log("Amount of walkers BEFORE clearing: " + walkers.Count);
		walkers.Clear();
		//Debug.Log("Amount of walkers AFTER clearing: " + walkers.Count);

		walker = new RandomWalker();
		walker.dir = Vector2.zero;


		for (int x = 0; x < levelWidth - 1; x++)
		{
			for (int y = 0; y < levelHeight - 1; y++)
			{
                if (grid[x,y] == LevelTile.empty)
                {
					walker.pos = new Vector2(x, y);
					//Debug.Log("Position of Walker: " + walker.pos);
                }
			}
		}

		walkers.Add(walker);
	}

	void CreateFloors() {
		int iterations = 0;
		do{
			//assign floor enum member at position of every Walker
			foreach (RandomWalker walker in walkers){
				grid[(int)walker.pos.x,(int)walker.pos.y] = LevelTile.floor;
			}

			//chance: destroy random Walker in list
			//int numberChecks = walkers.Count;
			//for (int i = 0; i < numberChecks; i++) {
			//	if (Random.value < chanceWalkerDestoy && walkers.Count > 1){
			//		walkers.RemoveAt(i);
			//		break;
			//	}
			//}

			//chance: Walker pick new direction
			for (int i = 0; i < walkers.Count -1; i++) {
				if (Random.value < chanceWalkerChangeDir){
					RandomWalker thisWalker = walkers[i];
					thisWalker.dir = RandomDirection();
					walkers[i] = thisWalker;

					currentWalkerPosition = thisWalker.pos;
				}
			}

			//chance: spawn new Walker
			//numberChecks = walkers.Count;
			//for (int i = 0; i < numberChecks; i++){
			//	if (Random.value < chanceWalkerSpawn && walkers.Count < maxWalkers) {
			//		RandomWalker walker = new RandomWalker();
			//		walker.dir = RandomDirection();
			//		walker.pos = walkers[i].pos;
			//		walkers.Add(walker);
			//	}
			//}

			//move Walkers
			for (int i = 0; i < walkers.Count; i++){
				RandomWalker walker = walkers[i];
				walker.pos += walker.dir;
				walkers[i] = walker;				
			}

			//avoid boarder of grid
			for (int i =0; i < walkers.Count; i++){
				RandomWalker walker = walkers[i];
				walker.pos.x = Mathf.Clamp(walker.pos.x, 1, levelWidth-2);
				walker.pos.y = Mathf.Clamp(walker.pos.y, 1, levelHeight-2);
				walkers[i] = walker;
			}

			//check to exit loop
			if ((float)NumberOfFloors() / (float)grid.Length > percentToFill){
				break;
			}
			iterations++;
		} while(iterations < iterationSteps);
	}

	void CreateWalls()
	{
		for (int x = 0; x < levelWidth-1; x++) 
		{
			for (int y = 0; y < levelHeight-1; y++) 
			{
				//This will look around every level tile that is a floor to see whether there needs to be a wall placed
				if (grid[x,y] == LevelTile.floor) {
					if (grid[x,y+1] == LevelTile.empty) //empty above given floor
					{ 
						grid[x,y+1] = LevelTile.wall;
					}

					if (grid[x,y-1] == LevelTile.empty) //empty under given floor
					{ 
						grid[x,y-1] = LevelTile.wall;
					}
					if (grid[x+1,y] == LevelTile.empty) //empty right of given floor
					{ 
						grid[x+1,y] = LevelTile.wall;
					}
					if (grid[x-1,y] == LevelTile.empty) //empty left of given floor
					{ 
						grid[x-1,y] = LevelTile.wall;
					}

                    if (grid[x - 1, y - 1] == LevelTile.empty) //empty left under
					{ 
                        grid[x - 1, y - 1] = LevelTile.wall;
                    }
                    if (grid[x - 1, y + 1] == LevelTile.empty) //empty left above
					{ 
                        grid[x - 1, y + 1] = LevelTile.wall;
                    }
                    if (grid[x + 1, y + 1] == LevelTile.empty) //empty right above
					{ 
                        grid[x + 1, y + 1] = LevelTile.wall;
                    }
                    if (grid[x + 1, y - 1] == LevelTile.empty) //empty right under
					{ 
                        grid[x + 1, y - 1] = LevelTile.wall;
                    }
                }
            }
		}
	}



	void CreateBottomWalls() {
		for (int x = 0; x < levelWidth; x++) {
			for (int y = 1; y < levelHeight; y++) {
				if (grid[x, y] == LevelTile.wall && grid[x, y - 1] == LevelTile.floor) {
                    grid[x, y] = LevelTile.bottomWall;
                }
            }
		}
	}

	void SpawnLevel(){
		for (int x = 0; x < levelWidth; x++) {
			for (int y = 0; y < levelHeight; y++) {
				//spawn the correct sprites on the tile that is specified in the grid via other methods
				switch(grid[x, y]) {
					case LevelTile.empty:
						break;
					case LevelTile.start:
						Spawn(x, y, start);
						break;
					case LevelTile.floor:
						Spawn(x, y, floorTiles[Random.Range(0, floorTiles.Length)]);
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
	{
		//int choice = Mathf.FloorToInt(Random.value * 3.99f); 

		int choice = Random.Range(0,4); //Chooses a random direction (north = 0, east = 1, south = 2, west = 3)

		int x = Mathf.FloorToInt(currentWalkerPosition.x);
		int y = Mathf.FloorToInt(currentWalkerPosition.y);

		Debug.Log("Walker Position = (" + x + ", " + y + ")");
		//Debug.Log("Current Walker Position is: " + currentWalkerPosition);

		bool northEmpty = false, eastEmpty = false, southEmpty = false, westEmpty = false;

		//current location of the walker on the grid
		Debug.Log("Where is the walker standing on? -> " + grid[x,y]);
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

        if (northEmpty == true || eastEmpty == true || southEmpty == true || westEmpty == true)
        {
            switch (choice)
            {
                case 0:
					Debug.Log("DOWN");
					return Vector2.down;
                case 1:
					Debug.Log("LEFT");
					return Vector2.left;
                case 2:
					Debug.Log("UP");
					return Vector2.up;
                default:
					Debug.Log("RIGHT");
					return Vector2.right;
            }
        }
		else
		{
			//KILL WALKER
			Debug.Log("KILL WALKER");
			AddNewWalker();
			return Vector2.zero;
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

	void SpawnStartTile()
    {
		Vector2 startTile = playerSpawnpoint;
		Spawn(startTile.x, startTile.y, start);
	}

    void SpawnPlayer() 
	{
		Vector2 playerPos = playerSpawnpoint;
		GameObject playerObj = Instantiate(player, playerPos, Quaternion.identity) as GameObject;
    }


    public void SpawnExit() 
	{

        Vector2 playerPos = new Vector2(Mathf.RoundToInt(levelWidth/ 2.0f), Mathf.RoundToInt(levelHeight/ 2.0f));
        Vector2 exitPos = playerPos;
        float exitDistance = 0f;

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