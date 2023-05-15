using Comentsys.Assets.FluentEmoji;
namespace Blazor.Emoji.Bingo;

public class Column
{
    public Column(FluentEmojiType primary, FluentEmojiType secondary) =>
        (Primary, Secondary) = (primary, secondary);

    public FluentEmojiType Primary { get; set; }

    public FluentEmojiType Secondary { get; set; }
}

public class Row
{
    public List<Column> Columns { get; set; } = new();
}

public class Display
{
    public List<Row> Rows { get; private set; } = new();
}

// Bingo Class
public class Bingo
{
    // Constants
    private const int size = 5;
    private const int rows = 10;
    private const int columns = 9;
    private const int delay = 3;
    private const int minimum = 1;
    private const int maximum = 90;

    // Members
    private FluentEmojiType[] _displayEmoji = Array.Empty<FluentEmojiType>();
    private FluentEmojiType[] _currentEmoji = Array.Empty<FluentEmojiType>();
    private List<int> _currentValues = new();
    private List<int> _displayValues = new();
    private Timer? _timer;
    private int _interval;
    private int _index;

    // Properties
    public int Players { get; set; } = 1;

    public int Player { get; set; } = 1;

    public int Winner { get; set; } = -1;

    public int Countdown { get; set; }

    public long Value { get; set; }

    public bool IsReady { get; set; }

    public string? Message { get; set; }

    public Action? Updated { get; set; }

    public Display Display { get; set; } = new();

    public Display Current { get; set; } = new();

    public List<List<int>> Tickets { get; set; } = new();

    // Choose & Get Methods
    private static List<int> Choose(int total, int value)
    {
        var random = new Random(value);
        return Enumerable.Range(minimum, maximum)
        .OrderBy(r => random.Next(minimum, maximum))
        .Take(total).ToList();
    }

    private static FluentEmojiType[] Get(List<int> values)
    {
        var emoji = Enum.GetNames<FluentEmojiType>()
        .Where(item => item.Contains("Face"))
        .Select(Enum.Parse<FluentEmojiType>).ToArray();
        return values.Select(value => emoji[value]).ToArray();
    }

    // Swap Method
    private static void Swap(Display display, FluentEmojiType emoji, bool swapPrimary)
    {
        var query = display.Rows.SelectMany(r => r.Columns);
        var column = swapPrimary ?
        query.FirstOrDefault(c => c.Secondary == emoji) :
        query.FirstOrDefault(c => c.Primary == emoji);
        if (column != null)
        {
            (column.Primary, column.Secondary) =
            (column.Secondary, column.Primary);
        }
    }

    // Layout Method
    private static void Layout(Display display, int rows, int columns,
        FluentEmojiType[]? list, FluentEmojiType item, bool isPrimary)
    {
        if (rows * columns == list?.Length)
        {
            int index = 0;
            display.Rows.Clear();
            for (int r = 0; r < rows; r++)
            {
                var row = new Row();
                for (int c = 0; c < columns; c++)
                {
                    var primary = isPrimary ? list[index] : item;
                    var secondary = isPrimary ? item : list[index];
                    row.Columns.Add(new Column(primary, secondary));
                    index++;
                }
                display.Rows.Add(row);
            }
        }
    }

    // Call Method
    private void Call()
    {
        Swap(Display, _displayEmoji[_index], true);
        Swap(Current, _displayEmoji[_index], false);
        var call = _displayValues[_index];
        Tickets.ForEach(w => w.Remove(call));
        Winner = Tickets.FindIndex(w => !w.Any()) + 1;
        if (Winner > 0)
        {
            Message = Winner == Player ?
            $"Your Player {Player} Bingo!" :
            $"Your Player {Player} Lost, Player {Winner} Bingo!";
            _timer?.Dispose();
        }
    }

    // Callback Method
    private void Callback(object? state)
    {
        if (Countdown < 0)
        {
            if (_interval >= delay)
            {
                if (_index < maximum)
                {
                    Call();
                    _interval = 0;
                    _index++;
                }
            }
            else
            {
                _interval++;
            }
        }
        else
        {
            Countdown--;
        }
        Updated?.Invoke();
    }

    // Ready Method
    public void Ready()
    {
        _index = 0;
        _interval = 0;
        Winner = -1;
        Tickets = new();
        Countdown = (int)(new DateTime(Value) - DateTime.UtcNow).TotalSeconds;
        _displayValues = Choose(maximum, (int)Value);
        _displayEmoji = Get(_displayValues);
        for (int i = 0; i < Players; i++)
        {
            Tickets.Add(Choose(size * size, i));
        }
        if (Player - 1 < Players)
        {
            _currentValues = Tickets[Player - 1];
            _currentEmoji = Get(_currentValues);
            Layout(Display, rows, columns, _displayEmoji,
                FluentEmojiType.HollowRedCircle, false);
            Layout(Current, size, size, _currentEmoji,
                FluentEmojiType.CrossMark, true);
            _timer = new Timer(Callback, null, 0, 1000);
            IsReady = true;
        }
    }

    // New Method & Constructor
    public void New()
    {
        _timer?.Dispose();
        IsReady = false;
        Value = DateTime.UtcNow.AddMinutes(1).Ticks;
    }

    public Bingo() => New();
}

