using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class GameScreen : Control
{
	private (Button cellButton, bool isMine, int adjacentMineCount, bool isRevealed, bool isFlagged)[,] cells; // could try & do a 2D list, but since there's only ever 10x15 buttons in the game, going to just wok with a 2D array, so i can do (...)[row][col] in for loops.
	// aha, https://stackoverflow.com/questions/12567329/multidimensional-array-vs has explained that (...)[][] cells is jagged/array of arrays, as you do ...cells = new (...)[x][];, & then for each cell, you do cells[0] = new (...)[y];, so it can be jagged looking. the (...)[,] is multidimensional, & uniform.
    // as chatgpt pointed out this couldve been a class rather than a huge list of tuples. i was even starting to think of a struct.
	private bool minesPlanted, gameFinished;
	private List<(int col, int row)> cellsClicked;
    private int setNoOfMines = Global.Instance.NoOfMines;

    private Label LblUpwardCountingTimer, LblNoOfClicks;
    private double timeElapsedInSecs;



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        Debug.Print($"------------------------ Started  _Ready(). ------------------------");
        cells = InitialiseCells(columns: 15, rows: 10); // not sure if i should do it this way or just do cells = in the InitialiseCells method, marking that method as a return type void. but i guess it's more flexible. also remembered somewhere someone writing the (variable1: value1, variable2: value2...) & i feel it suits here for readability.
        minesPlanted = false; gameFinished = false;
        cellsClicked = new List<(int col, int row)>();

        InitialiseTimerLabel();
        InitialiseNoOfClicksLabel();

        Debug.Print($"------------------------ Finished _Ready(). ------------------------");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (cellsClicked.Count > 0 && !gameFinished)
        {
            UpdateTimingLabel(delta);
        }
	}

    private void UpdateTimingLabel(double delta)
    {
        // https://www.reddit.com/r/godot/comments/1e1pj76/help_timer_that_counts_up_and_down/
        timeElapsedInSecs += delta;
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeElapsedInSecs); // https://stackoverflow.com/questions/40867158/how-can-i-format-a-float-number-so-that-it-looks-like-real-time
        string labelText = $"Time: {timeSpan:m\\:ss\\.fff}";
        LblUpwardCountingTimer.Text = labelText;
        Debug.Print(labelText);
    }
    
    private void UpdateNoOfClicksLabel()
    {
        string labelText = $"No. Of Clicks: {cellsClicked.Count}";
        LblNoOfClicks.Text = labelText;
        Debug.Print(labelText);
    }

    #region Initialisation
    private (Button cellButton, bool isMine, int adjacentMineCount, bool isRevealed, bool isFlagged)[,] InitialiseCells(int columns, int rows)
	{
        Debug.Print($"Started InitialiseCells()");


        (Button cellButton, bool isMine, int adjacentMineCount, bool isRevealed, bool isFlagged)[,] initialisedCells = new (Button, bool, int, bool, bool)[columns, rows];
		var elementDimensions = GetElementsDimensions();
        Debug.Print($"There are {columns} columns & {rows} rows.");
        for (int col = 0; col<columns; col++)
		{
			for (int row = 0; row<rows; row++)
			{
				initialisedCells[col, row].cellButton = new Button()
				{
					Position = new Vector2()
					{
						X = elementDimensions.windowSidesMarginGap + (elementDimensions.cellGapWidth * (float)col) + (elementDimensions.cellWidth * (float)col),
						Y = elementDimensions.windowCeilingGap + (elementDimensions.cellGapHeight * (float)row) + (elementDimensions.cellHeight * (float)row),
					},
					Size = new Vector2()
					{
						X = elementDimensions.cellWidth,
						Y = elementDimensions.cellHeight,
					}
				};
				int currentRow = row, currentCol = col;

                //initialisedCells[col, row].cellButton.Pressed += () => // https://docs.godotengine.org/en/stable/classes/class_signal.html#signal & used ChatGPT for the lambda expression trick. knew of it, just havent brushed up on it in a while, so now i know.
                //{
                //    _OnCellClicked(currentCol, currentRow);
                //    //Debug.Print($"Button clicked at column {col} & row {row}.");
                //};

                initialisedCells[currentCol, currentRow].cellButton.GuiInput += (InputEvent e) => // thank you chatgpt for basically this entire bit of code to also process right clicks
                {
                    if (e is InputEventMouseButton mouse &&
                        mouse.Pressed)
                    {
                        if (mouse.ButtonIndex == MouseButton.Left)
                        {
                            _OnCellClicked(currentCol, currentRow);
                        }
                        else if (mouse.ButtonIndex == MouseButton.Right)
                        {
                            Debug.Print($"Flagged cell @ Col:{currentCol}, Row:{currentRow}");
                            ToggleFlagged(currentCol, currentRow);
                        }
                    }
                };

                Debug.Print($"Button is generated at column {currentCol} & row {currentRow}, at co-ords X:{initialisedCells[currentCol, currentRow].cellButton.Position.X}, Y:{initialisedCells[currentCol, currentRow].cellButton.Position.Y}");


                this.AddChild(initialisedCells[currentCol, currentRow].cellButton); // & add the button to the scene, nearly forgot.

				initialisedCells[currentCol, currentRow].isMine = false;
				initialisedCells[currentCol, currentRow].adjacentMineCount = 0;
				initialisedCells[currentCol, currentRow].isRevealed = false;
				initialisedCells[currentCol, currentRow].isFlagged = false;

				//Debug.Print($"Cell [Column {col}, Row {row}] positioned at X={initialisedCells[col, row].cellButton.Position.X} & Y={initialisedCells[col, row].cellButton.Position.Y}");
			}

        }

        Debug.Print($"Ended InitialiseCells()");
        return initialisedCells;
    }

    private void InitialiseTimerLabel()
    {
        Debug.Print($"Started InitialiseTimerLabel()");

        timeElapsedInSecs = 0d;

        var dims = GetElementsDimensions();
        LblUpwardCountingTimer = new Label()
        {
            Name = "LblUpwardCountingTimer",
            Position = new Vector2()
            {
                X = this.GetViewportRect().Size.X / 7f,
                Y = dims.windowCeilingGap / 20f
            },
            Size = new Vector2()
            {
                X = this.GetViewportRect().Size.X * (2f/7f),
                Y = dims.windowCeilingGap * (9f/10f)
            }, // so height positioning & size wise, theres a 20th of the margin above & below the label
             HorizontalAlignment = HorizontalAlignment.Center,
             VerticalAlignment = VerticalAlignment.Center,
             Text = "Time: 00:00"
        };
        LblUpwardCountingTimer.Set("theme_override_colors/font_color", new Color(1,1,1));

        this.AddChild(LblUpwardCountingTimer);

        Debug.Print($"LblUpwardCountingTimer Position: X:{LblUpwardCountingTimer.Position.X}, Y:{LblUpwardCountingTimer.Position.Y}");
        Debug.Print($"LblUpwardCountingTimer Size: X:{LblUpwardCountingTimer.Size.X}, Y:{LblUpwardCountingTimer.Size.Y}");
        Debug.Print($"Ended InitialiseTimerLabel()");
    }

    private void InitialiseNoOfClicksLabel()
    {
        var dims = GetElementsDimensions();
        LblNoOfClicks = new Label()
        {
            Name = "LblNoOfClicks",
            Position = new Vector2()
            {
                X = this.GetViewportRect().Size.X * (4f / 7f),
                Y = dims.windowCeilingGap / 20f
            },
            Size = new Vector2()
            {
                X = this.GetViewportRect().Size.X * (2f / 7f),
                Y = dims.windowCeilingGap * (9f / 10f)
            }, // so height positioning & size wise, theres a 20th of the margin above & below the label
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = "No. Of Clicks: 0"
        };
        LblNoOfClicks.Set("theme_override_colors/font_color", new Color(1, 1, 1));

        this.AddChild(LblNoOfClicks);
    }
    #endregion

    #region DrawScreen
    private (float cellWidth, float cellHeight, float cellGapWidth, float cellGapHeight, float windowCeilingGap, float windowFloorGap, float windowSidesMarginGap) GetElementsDimensions()
    {
        Debug.Print($"Started GetElementsDimensions()");

        /*
            To be able to express the elements on screen using a relative measure for whatever size the screen is resized to, take 'x' to be a cell's width, & 'y' to be a cell's height.
            Horizontally:
                margin between left side of window to leftmost cell = x/10
                15 cells horizontally = 15x
                14 gaps between 15 cells = 14x/8 = 7x/4, therefore 1 horizontal gap is x/8
                margin between rightmost cell to right side of window = x/10
            Vertically:
                margin between window ceiling to topmost cell = 11y/10
                10 cells vertically = 10y
                9 gaps between 10 cells = 9y/8, therefore 1 vertical gap is y/8
                margin between bottommost cell to window floor = y/10
            Thus:
                Horizontal total = x/10 + 15x + 7x/4 + x/10 = 2x/20 + 300x/20 + 35x/20 + 2x/20 = 339x/20 = Viewport Width, therefore x = 20/339 * Viewport Width
                Vertical total = 11y/10 + 10y + 9y/8 + y/10 = 88y/80 + 800y/80 + 90y/80 + 8y/80 = 986y/80 = 99y/8 = Viewport Width, therefore y = 8/99 * Viewport Width
        */
        float cellWidth = this.GetViewportRect().Size.X * (20f / 339f),
            cellHeight = this.GetViewportRect().Size.Y * (8f / 99f); // foergot to add f to 8 & 99 which is why all the y levels for buttons were 0, all cellheights were 0. this was the only setback from achieving it 1st try.

        float windowCeilingGap = cellHeight * (11f / 10f),
            windowFloorGap = cellHeight / 10f,
            windowSidesMarginGap = cellWidth / 10f,
            cellGapWidth = cellWidth / 8f,
            cellGapHeight = cellHeight / 8f;

        Debug.Print($"Values are:" +
            $"\t cellWidth={cellWidth}" +
            $"\t this.GetViewportRect().Size.Y={this.GetViewportRect().Size.Y}" +
            $"\t this.GetViewportRect().Size.Y * (8 / 99) = cellHeight={cellHeight}" +
            $"\t windowCeilingGap={windowCeilingGap}" +
            $"\t windowSidesMarginGap={windowSidesMarginGap}" +
            $"\t cellGapWidth={cellGapWidth}" +
            $"\t cellGapHeight={cellGapHeight}");
        Debug.Print($"Ended GetElementsDimensions()");

        return (cellWidth, cellHeight, cellGapWidth, cellGapHeight, windowCeilingGap, windowFloorGap, windowSidesMarginGap);
    }

	private void UpdateCellsVisibility()
	{
        Debug.Print($"Started UpdateCellsVisibility()");

        int columns = cells.GetLength(0), rows = cells.GetLength(1);
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if(cells[col, row].isFlagged)
                {
                    cells[col, row].cellButton.Text = "⛳";
                }
                else
                {
                    cells[col, row].cellButton.Text = "";
                }

                if (cells[col, row].isRevealed && cells[col, row].adjacentMineCount > 0) // mine-bordering buttons
                {
                    cells[col, row].cellButton.Set("theme_override_styles/disabled/bg_color", new StyleBoxFlat()
                    {
                        BgColor = new Color(0.5f, 0.7f, 0.5f)
                    });
                    cells[col, row].cellButton.Set("theme_override_colors/font_color", new Color(1, 0.96f, 0.11f, 1));

                    cells[col, row].cellButton.Text = $"{cells[col, row].adjacentMineCount}";
                    cells[col, row].cellButton.Disabled = true;
                }
				else if (cells[col, row].isRevealed && cells[col, row].adjacentMineCount == 0) // regular empty buttons
				{
                    cells[col, row].cellButton.Set("theme_override_styles/disabled/bg_color", new StyleBoxFlat()
                    {
                        BgColor = new Color(0.2f, 0.8f, 0.2f)
                    });
                    cells[col, row].cellButton.Disabled = true;
                }
            }
        }

        Debug.Print($"Ended UpdateCellsVisibility()");
    }

    private void RevealAllMines()
    {
        Debug.Print($"Started RevealAllMines()");

        int columns = cells.GetLength(0), rows = cells.GetLength(1);
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (cells[col, row].isMine)
                {
                    cells[col, row].cellButton.Text = $"Mine";
                    
                    cells[col, row].cellButton.Set("theme_override_styles/disabled/bg_color", new StyleBoxFlat()
                    {
                        BgColor = new Color(0f, 0f, 0f)
                    });
                }
            }
        }

        Debug.Print($"Ended RevealAllMines()");
    }
    #endregion

    #region PlantMines
    private void PlantMines(int colInitialClick, int rowInitialClick) // randomly scatter mines after player clicks, taking into account 1st click coords to prevent player's 1st click being a mine
	{
        Debug.Print($"------------ PLANTING MINES ------------");
        Debug.Print($"Initial Cell Clicked: Column Index: {colInitialClick}, Row Index: {rowInitialClick}");

        int columns = cells.GetLength(0), rows = cells.GetLength(1); // fetch how big the gridmap of cells is, by rows & cols
        Debug.Print($"There are {columns} columns & {rows} rows.");

        int noOfCells = columns * rows; // calculate how many cells there are in total
        Debug.Print($"There are {noOfCells} cells total.");

        int noOfMines = setNoOfMines; // have the number of mines generated be a fraction of the total number of cells
        Debug.Print($"{noOfMines} mines will be planted.");

        string[] mineCoords = new string[noOfMines]; // each element will be in the form [{x1,y1}, {x2,y2}, {x3,y3}]... & helper methods of ParseMineCoords & GenerateMineCoordsString will be used to process I/Os

        Random random = new Random(); // prep a random variable
		for (int i = 0; i < noOfMines; i++) { // loop to generate (noOfMines) * mines for the grid, ensuring theyre all valid placements.
			bool validMinePlacement = false;
			int mineCol = -1, mineRow = -1;


            while (!validMinePlacement) // while we havent found a valid place to place the mine, keep looping until a valid place has been located, before moving onto planting the next mine.
			{
				mineCol = random.Next(0, columns); mineRow = random.Next(0, rows);

				if (Math.Max(mineCol, colInitialClick) - Math.Min(mineCol, colInitialClick) <= 1 &&
                    Math.Max(mineRow, rowInitialClick) - Math.Min(mineRow, rowInitialClick) <= 1) { continue; } // give the user's 1st click some space around their click so they dont click on numbers over & over until they find a large area to work off.
				else if(mineCoords.Contains(GenerateCoordsString(mineCol, mineRow))) { continue; } // or, if the generated coords already exist in the list of saved confirmed valid coords
				else { validMinePlacement = true; } // valid placement, add to list
            }

			mineCoords[i] = GenerateCoordsString(mineCol, mineRow);
		}

		for (int i = 0; i < mineCoords.Length; i++)
		{
			if (mineCoords[i] == null) { continue; }
			(int col, int row) = ParseCoords(mineCoords[i]);
			if (col == -1 || row == -1)
			{
				Debug.Print($"Error: Failed to plant Mine #{i+1}; Mine Coord @ index {i} failed to parse. Value at index: {mineCoords[i]}"); continue;
			}
			else
			{
                Debug.Print($"Planting Mine #{i + 1} @ Column {col}, Row {row}...");

                cells[col, row].isMine = true;
				Debug.Print($"Mine #{i+1} Planted at Column {col}, Row {row}.");
				FillCellsAdjacentToMineCount(col, row);
			}
		}

        Debug.Print($"------------ PLANTED  MINES ------------");
    }

	private void FillCellsAdjacentToMineCount(int mineCol, int mineRow)
	{
        Debug.Print($"Started FillCellsAdjacentToMineCount()");

        int columns = cells.GetLength(0), rows = cells.GetLength(1);

		(int upOffset, int rightOffset)[] allCombinations = { (1, -1), (1, 0), (1, 1), (0, -1), (0, 1), (-1, -1), (-1, 0), (-1, 1) };
		for (int i = 0; i<allCombinations.Length; i++)
		{
            int proposedCellCol = mineCol + allCombinations[i].rightOffset;
            int proposedCellRow = mineRow + allCombinations[i].upOffset;

            if (proposedCellCol>=0 && proposedCellCol<columns
				&& proposedCellRow>=0 && proposedCellRow<rows) // check if it's a valid cell to interact with, so as not to get the outofbounds error.
			{
				cells[proposedCellCol, proposedCellRow].adjacentMineCount++;
			}
		}

        Debug.Print($"Ended FillCellsAdjacentToMineCount()");
    }

    #endregion PlantMines

    #region CoordHelperMethods
    /// <summary>
    /// Processes the parameter mineCoordsString & returns a tuple of the column & row of that string.
    /// </summary>
    /// <param name="mineCoordsString"></param>
    /// <returns>(-1,-1) if failed to parse.</returns>
    private (int col, int row) ParseCoords(string mineCoordsString)
	{
        Debug.Print($"Started ParseCoords()");

        string[] coords = mineCoordsString.Split(',');
		int parsedCol, parsedRow;
		if (int.TryParse(coords[0], out int a)) {
			parsedCol = a;
		} else
		{
			parsedCol = -1;
		}

        if (int.TryParse(coords[1], out int b))
        {
            parsedRow = b;
        }
        else
        {
            parsedRow = -1;
        }

        Debug.Print($"Ended ParseCoords()");

        return (parsedCol, parsedRow);
    }
	/// <summary>
	/// Takes 2 inputs to form a coordinate represented by string
	/// </summary>
	/// <param name="col"></param>
	/// <param name="row"></param>
	/// <returns>A string of 2 integers seperated by a comma, i.e: int1,int2</returns>
	private string GenerateCoordsString(int col, int row)
	{
        Debug.Print($"Started & Ended GenerateCoordsString()");

        return $"{col},{row}";
	}
    #endregion MineCoordHelperMethods

    private void _OnCellClicked(int colClicked, int rowClicked)
	{
        Debug.Print($"-----------------------------------------------------------------------------");
        Debug.Print($"Started _OnCellClicked(); Cell Clicked: Column {colClicked}, Row {rowClicked}");

		if(cellsClicked.Contains((colClicked, rowClicked)) || cells[colClicked, rowClicked].isFlagged)
		{
			return;
		}

        Debug.Print($"No. of Cells Clicked so far: {cellsClicked.Count}");
        cellsClicked.Add( (colClicked, rowClicked) ); // add currently clicked cell to list

        /*
		 * if 1st click:
		 *		generate mines
		 *		floodreveal cells
		 *		paint 1st clicked cell as click #1
		 *		check if all remaining unrevealed squares are mines. if they are:
		 *			GameWin(); prevent button clicks & display gamewin prompt/screen.
		 *		otherwise, logic ends, & game listens for next click.
		 * otherwise:
		 *		if clicking an already revealed cell
		 *			Dont react, end logic, & listen for next click.
		 *		otherwise:
		 *			floodreveal cells
		 *			paint clicked cell as #n
		 *			check if clicked cell is a mine. if it is:
		 *				GameLose(); prevent any further button inputs & display gamelose prompt/screen.
		 *			otherwise, if cell is safe,:
		 *				check if all remaining unrevealed squares are mines. if they are:
		 *					GameWin()
		 *				otherwise (if some unrevealed cells are safe):
		 *					end logic, & listen for next click.
		*/

        UpdateNoOfClicksLabel();

        if (!minesPlanted) // for initial click
		{
			PlantMines(colClicked, rowClicked);
			minesPlanted = true;
		} else if (IsClickedCellAlreadyRevealed(colClicked, rowClicked))
		{
            Debug.Print($"Cell already clicked before.");
            return; // player clicked a cell that was already visible; dont respond. just listen for the next click.
		}
        

		// reveal all cells that should be revealed based on latest click.
        Debug.Print($"About to flood reveal after click #{cellsClicked.Count}...");
        FloodReveal(colClicked, rowClicked); // reveal all cells from that click
        Debug.Print($"Flood revealed map after click #{cellsClicked.Count}.");
        
		UpdateCellsVisibility();

		if (IsCellAMine(colClicked, rowClicked))
		{
			GameLost(); return;
		}
		else if(AreAllRemainingSpacesMines())
		{
			GameWon(); return;
		}

        Debug.Print($"Ended _OnCellClicked()");
        Debug.Print($"-----------------------------------------------------------------------------");
    }
    private bool IsCellAMine(int requestedCol, int requestedRow)
	{
        Debug.Print($"Started & Ended IsCellAMine()");

        return cells[requestedCol, requestedRow].isMine;
	}
	private void FloodReveal(int colClicked, int rowClicked)
	{
        Debug.Print($"Started FloodReveal()");

		/*
		 *	Originally planned to go from 1 cell & search all 8 cells around it, but i decided to just do 4 directions & send the algoritm drilling along each of the 4 directions, such that at each next cell it travels along incrementally, it spawns another 4-directional algorithm drilling machine again.
			But then i realised the way that drilling machine algorithm works means you spawn one new algorithm searcher every cell, so if a neighbouring cell an instance of the algorithm  is	inspecting is already revealed, then that means the algorithm has already started from there at some point, because it'll only make the square it's		currently on or an	adjacent	 one which has a mine count >0, revealed, check for all adjacent undiscovered cells, spawn a new algorithm there, & once all it's	adjacent cells have been	revealed, it stops its	 specific instance of the algorithm.
			But i realise that searching diagonals is needed too. So the flowcharty bulletpoint look at it ive come up with is as follows:
			
			- Start at one cell, say, (x1, y1)
			- Reveal that cell.
			- Search in a clockwise motion up-middle, up-right, right-middle, right-down, ..., left-middle, & left-up.
			- If any of those neighbouring cells are undiscovered with an adjacent mine count == 0, start a new instance of the algorithm there.
			- If any neighbouring cells are undiscovered & have an adjacent mine count > 0, reveal that cell, & continue searching neighbouring cells in the clockwise fashion.
			- If any neighbouring cells are already revealed, skip those & continue searching neighbouring cells in the clockwise fashion.
			- Once you make it from searching up-middle to left-up from (x1, y1), end that instance of the algorithm.
			
			& theoretically, this would spawn enough instances to reveal the whole valid area from where you clicked.
            
            additionally, while testplaying, if the cell clicked is a numbered cell, then just reveal that & no more, cause ive been accidentally filling loads of numbered cells when clicking a cell.
		*/

		cells[colClicked, rowClicked].isRevealed = true; // darn it i made this space to write this line but i completely forgot to write it.
        if(cells[colClicked, rowClicked].adjacentMineCount > 0)
        {
            return;
        }

        int columns = cells.GetLength(0), rows = cells.GetLength(1);
        (int upOffset, int rightOffset)[] allOffsets = { (1, -1), (1, 0), (1, 1), (0, -1), (0, 1), (-1, -1), (-1, 0), (-1, 1) };

        for(int i = 0; i<allOffsets.Length; i++) {
			int proposedCellCol = colClicked + allOffsets[i].rightOffset, proposedCellRow = rowClicked + allOffsets[i].upOffset;
            if (proposedCellCol >= 0 && proposedCellCol < columns
                && proposedCellRow >= 0 && proposedCellRow < rows) // check if it's safe to do checks on without going out of bounds. if not, then it's not a real cell, out of bounds, so ignore without worry.
			{
				var cellBeingInspected = cells[proposedCellCol, proposedCellRow];

				if(cellBeingInspected.isMine || cellBeingInspected.isFlagged)
				{
					continue; // skip processing isMine &/ if it's disabled (which would be because it could be a flag planted), dont reveal or anything.
				}

                if (!cellBeingInspected.isRevealed) // is undiscovered
                {
					if(cellBeingInspected.adjacentMineCount == 0) // & not touching any mines
                    {
						FloodReveal(proposedCellCol, proposedCellRow);
					}
                    if (cellBeingInspected.adjacentMineCount > 0) // & touching 1+ mines
                    {
                        cells[proposedCellCol, proposedCellRow].isRevealed = true; // reveal that inspected cell & continue on to next cell to inspect.
                    }
                }

				// i.e. if cell being inspected has already been revealed, ignore it & continue on with searching in all other directions.
			}
        }

        Debug.Print($"Ended FloodReveal()");
    }
    private bool AreAllRemainingSpacesMines()
	{
        Debug.Print($"Started AreAllRemainingSpacesMines()");

        // just search for 1 unrevealed safe cell to prove a return of false.
        int columns = cells.GetLength(0), rows = cells.GetLength(1);
		for (int col = 0; col < columns; col++)
		{
			for (int row = 0; row < rows; row++)
			{
				if (!cells[col, row].isRevealed && !cells[col, row].isMine) // isnt revealed & isnt a mine; doesnt work for some reason.
				{
                    Debug.Print($"AreAllRemainingSpacesMines() - Undiscovered safe cell located at Column{col} Row{row}");

                    return false;
				}
			}
		}

        Debug.Print($"Ended AreAllRemainingSpacesMines()");

        return true;
	}
	private bool IsClickedCellAlreadyRevealed(int requestedCol, int requestedRow)
	{
        Debug.Print($"Started & Ended IsClickedCellAlreadyRevealed()");

        return cells[requestedCol, requestedRow].isRevealed;
    }

	private void GameLost()
	{
        Debug.Print($"Started GameLost()");

        gameFinished = true;

        Save currentGameAsSave = CurrentGameAsSave(false);
        SaveManager.AddSave(currentGameAsSave);

        RevealAllMines();
        DisableAllButtons();

        GameEndAcceptDialog($"You lose." +
            $"\n\tTime Taken:  {currentGameAsSave.TimeTaken:m\\:ss\\.fff}" +
            $"\n\tClicks Taken: {currentGameAsSave.NoOfClicks}" +
            $"\n\tDifficulty: {currentGameAsSave.Difficulty}" +
            $"\nPlay again?",
            $"Yes, play again");

        Debug.Print($"Ended GameLost()");
    }
    private void GameWon()
	{
        Debug.Print($"Started GameWon()");

        gameFinished = true;

        Save currentGameAsSave = CurrentGameAsSave(true);
        SaveManager.AddSave(currentGameAsSave);

        GameEndAcceptDialog($"You win!" +
            $"\n\tTime Taken:  {currentGameAsSave.TimeTaken:m\\:ss\\.fff}" +
            $"\n\tClicks Taken: {currentGameAsSave.NoOfClicks}" +
            $"\n\tDifficulty: {currentGameAsSave.Difficulty}" +
            $"\nPlay again?", $"Yes, play again");

        Debug.Print($"Ended GameWon()");
    }

    private Save CurrentGameAsSave(bool isWin)
    {
        TimeSpan timeTaken = TimeSpan.FromSeconds(timeElapsedInSecs);
        int noOfClicks = cellsClicked.Count;

        Save.GameDifficulty gameDifficulty;
        switch(setNoOfMines)
        {
            case 10: gameDifficulty = Save.GameDifficulty.Easy; break;
            case 15: gameDifficulty = Save.GameDifficulty.Medium; break;
            case 20: gameDifficulty = Save.GameDifficulty.Hard; break;
            case 40: gameDifficulty = Save.GameDifficulty.VeryHard; break;
            default: gameDifficulty = Save.GameDifficulty.Unknown; break;
        }

        return new Save()
        {
            TimeTaken = timeTaken,
            NoOfClicks = noOfClicks,
            Difficulty = gameDifficulty,
            IsWin = isWin,
            SaveDateTime = DateTime.Now,
            SaveNum = 0
        };
    }

    private void GameEndAcceptDialog(string dialogText, string okBtnText)
    {
        var dims = GetElementsDimensions();
        AcceptDialog ad = new AcceptDialog()
        {
            Visible = true,
            DialogText = dialogText,
            OkButtonText = okBtnText,
            Size = new Vector2I((int)(dims.cellWidth * 5), (int)(dims.cellHeight * 4))
        };

        AddChild(ad);
        ad.PopupCentered();

        ad.CloseRequested += () =>
        {
            GetTree().ChangeSceneToFile("MainMenu.tscn");
        };
        ad.Confirmed += () =>
        {
            GetTree().ReloadCurrentScene();
        };
    }

    private void DisableAllButtons()
    {
        int columns = cells.GetLength(0), rows = cells.GetLength(1);
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                cells[col, row].cellButton.Disabled = true;
            }
        }
    }


    private void ToggleFlagged(int colClicked, int rowClicked)
    {
        if(!cells[colClicked, rowClicked].isRevealed) // only real condition is to ensure it isnt revealed, to allow player to flag the cell.
        {
            cells[colClicked, rowClicked].isFlagged = !cells[colClicked, rowClicked].isFlagged;
            UpdateCellsVisibility(); // only need to update visibility of cells when something changes, i.e. here, that means in the if statement.
        }
    }
}
