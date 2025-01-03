namespace BattleShipsApp;

public partial class MainPage : ContentPage
{
    private Random rnd = new Random();
    private List<(int Row, int Column)> aiShips = new();
    private List<(int Row, int Column)> selectedShips = new(); 
    private List<(int Row, int Column)> attackedPositions = new();
    private List<(int Row, int Column)> aiAttackedPositions = new(); 

    private bool isUserTurn = true;
    private bool gameOver = false;

    public MainPage()
    {
        InitializeComponent();
        GenerateAIShips();
        InitializePlacementGridInteractions();
        InitializeAttackGridInteractions();
    }

    private void GenerateAIShips()
    {
        aiShips.Clear();
        while (aiShips.Count < 3)
        {
            int row = rnd.Next(0, 4);
            int column = rnd.Next(0, 4); 

            var position = (row, column);
            if (!aiShips.Contains(position))
            {
                aiShips.Add(position);
            }
        }
    }

    private void InitializePlacementGridInteractions()
    {
        foreach (var child in PlacementGrid.Children)
        {
            if (child is BoxView box)
            {
                var row = Grid.GetRow(box);
                var column = Grid.GetColumn(box);

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnPlacementBoxTapped(box, row, column);
                box.GestureRecognizers.Add(tapGesture);
            }
        }

        ConfirmButton.Clicked += OnConfirmButtonClicked;
    }

    private void OnPlacementBoxTapped(BoxView box, int row, int column)
    {
        if (selectedShips.Contains((row, column)))
        {
            selectedShips.Remove((row, column));
            box.Color = Colors.Gray;
        }
        else if (selectedShips.Count < 3)
        {
            selectedShips.Add((row, column));
            box.Color = Colors.Orange;
        }

        ConfirmButton.IsEnabled = selectedShips.Count == 3;
    }

    private void OnConfirmButtonClicked(object sender, EventArgs e)
    {
        ConfirmButton.IsVisible = false;
        AttackGrid.IsVisible = true;
        AttackGridText.IsVisible = true;
    }

    private void InitializeAttackGridInteractions()
    {
        foreach (var child in AttackGrid.Children)
        {
            if (child is BoxView box)
            {
                var row = Grid.GetRow(box);
                var column = Grid.GetColumn(box);

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnAttackBoxTapped(box, row, column);
                box.GestureRecognizers.Add(tapGesture);
            }
        }
    }

    private void OnAttackBoxTapped(BoxView box, int row, int column)
    {
        if (gameOver || !isUserTurn || attackedPositions.Contains((row, column)))
        {
            return;
        }

        attackedPositions.Add((row, column));
        isUserTurn = false;

        if (aiShips.Contains((row, column)))
        {
            box.Color = Colors.Green;
            DisplayAlert("Hit", $"You hit a ship at ({row}, {column})!", "OK");
        }
        else
        {
            box.Color = Colors.Red;
            DisplayAlert("Miss", $"No ship at ({row}, {column}).", "OK");
        }

        CheckGameOver();

        if (!gameOver)
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                AIAttack();
                return false;
            });
        }
    }

    private void AIAttack()
    {
        if (gameOver) return;

        (int Row, int Column) position;

        do
        {
            position = (rnd.Next(0, 4), rnd.Next(0, 4));
        } while (aiAttackedPositions.Contains(position));

        aiAttackedPositions.Add(position);

        var box = GetBoxFromGrid(PlacementGrid, position.Row, position.Column);
        if (selectedShips.Contains(position))
        {
            box.Color = Colors.Green;
            DisplayAlert("AI Hit", $"AI hit your ship at ({position.Row}, {position.Column})!", "OK");
        }
        else
        {
            box.Color = Colors.Red;
            DisplayAlert("AI Miss", $"AI attacked ({position.Row}, {position.Column}) and missed.", "OK");
        }

        CheckGameOver();

        isUserTurn = true;
    }

    private void CheckGameOver()
    {
        if (attackedPositions.Count(pos => aiShips.Contains(pos)) == 3)
        {
            gameOver = true;
            VictoryText.Text = "Game Over, Congratulations! You have sunk all the AI ships!";
            ResetButton.IsVisible = true;
            ResetButton.Clicked += OnResetButtonTapped;
            return;
        }

        if (aiAttackedPositions.Count(pos => selectedShips.Contains(pos)) == 3)
        {
            gameOver = true;
            VictoryText.Text = "Game Over, The AI has sunk all your ships. Better luck next time!";
            ResetButton.IsVisible = true;
            ResetButton.Clicked += OnResetButtonTapped;
        }
    }

    private BoxView GetBoxFromGrid(Grid grid, int row, int column)
    {
        foreach (var child in grid.Children)
        {
            if (child is BoxView box &&
                Grid.GetRow(box) == row &&
                Grid.GetColumn(box) == column)
            {
                return box;
            }
        }
        return null;
    }
    
    private void OnResetButtonTapped(object sender, EventArgs e)
    {
        aiShips.Clear();
        selectedShips.Clear();
        attackedPositions.Clear();
        aiAttackedPositions.Clear();
        isUserTurn = true;
        gameOver = false;

        GenerateAIShips();
        foreach (var child in PlacementGrid.Children)
        {
            if (child is BoxView box)
            {
                box.Color = Colors.Gray;
            }
        }
        foreach (var child in AttackGrid.Children)
        {
            if (child is BoxView box)
            {
                box.Color = Colors.Gray;
            }
        }

        VictoryText.Text = string.Empty;
        ResetButton.IsVisible = false;

        ConfirmButton.IsVisible = true;
        AttackGrid.IsVisible = false;
        AttackGridText.IsVisible = false;
    }
}
