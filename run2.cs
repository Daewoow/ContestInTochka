using System;
using System.Collections.Generic;
using System.Linq;

class Edge
{
    public int KeysToNextNode;
    public int Dist;
    public int KeysNeededMask;
}

internal class State : IEquatable<State>
{
    public int[] Pos = Array.Empty<int>();
    public int Keys;
    public string Encode() => string.Join(",", Pos) + "|" + Keys;
    public bool Equals(State other) => other is not null && Encode() == other.Encode();
    public override bool Equals(object obj)
    {
        return Equals(obj as State);
    }
    public override int GetHashCode() => Encode().GetHashCode();
}

class Program
{
    // Константы для символов ключей и дверей
    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();

    private static int Rows;
    private static int Columns;
    private static List<Edge>[] Graph = Array.Empty<List<Edge>>();
    private static Dictionary<char, int> KeyIndexDict = new(); 

    // Метод для чтения входных данных
    static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;
        while ((line = Console.ReadLine()) != null && line != "") 
            data.Add(line.ToCharArray().ToList());
        return data;
    }

    static int Solve(List<List<char>> data)
    {
        Rows = data.Count;
        Columns = data[0].Count;
        
        var starts = new List<(int r, int c)>(4);
        var keyPos = new Dictionary<char, (int r, int c)>();
        for (var r = 0; r < Rows; ++r)
        {
            for (var c = 0; c < Columns; ++c)
            {
                var ch = data[r][c];
                switch (ch)
                {
                    case '@':
                        starts.Add((r, c));
                        data[r][c] = '.';
                        break;
                    case >= 'a' and <= 'z':
                        keyPos[ch] = (r, c);
                        break;
                }
            }
        }

        if (starts.Count != 4)
            throw new ArgumentException("Роботов неверное количество, проверьте введённые данные");
        var keysCount = keyPos.Count;
        KeyIndexDict = keyPos.Keys
            .OrderBy(c => c)
            .Select((c, i) => new { c, i })
            .ToDictionary(x => x.c, x => x.i);
        var totalKeysMask = (1 << keysCount) - 1;
        var nodesCount = starts.Count + keysCount;
        Graph = new List<Edge>[nodesCount];
        for (var i = 0; i < nodesCount; i++) 
            Graph[i] = new List<Edge>();

        for (var i = 0; i < starts.Count; ++i)
            BFS(data, i, starts[i], starts);
        
        foreach (var (key, value) in keyPos)
        {
            var keyIndex = KeyIndexDict[key];
            var nid = starts.Count + keyIndex;
            BFS(data, nid, value, starts);
        }

        return Dijkstra(starts, totalKeysMask, Graph);
    }
    
    private static void BFS(List<List<char>> data, int start, (int row, int column) source, List<(int r, int c)> starts)
    {
        var visited = new bool[Rows, Columns];
        var queue = new Queue<(int r, int c, int dist, int reqMask)>();
        queue.Enqueue((r: source.row, c: source.column, 0, 0));
        visited[source.row, source.column] = true;
        var dy = new[] { -1, 1, 0, 0 };
        var dx = new[] { 0, 0, -1, 1 };
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            for (var d = 0; d < 4; ++d)
            {
                var newRow = cur.r + dy[d];
                var newColumn = cur.c + dx[d];
                if (newRow < 0 || newRow >= Rows || newColumn < 0 || newColumn >= Columns) 
                    continue;
                if (visited[newRow, newColumn]) 
                    continue;
                var next = data[newRow][newColumn];
                if (next == '#') 
                    continue;
                var nextDoorMask = cur.reqMask;
                if (next >= 'A' && next <= 'Z')
                {
                    var doorIndex = Array.IndexOf(doors_char, next);
                    if (doorIndex >= 0) 
                        nextDoorMask |= 1 << doorIndex;
                }
                visited[newRow, newColumn] = true;
                if (next is >= 'a' and <= 'z')
                {
                    var nextIndex = KeyIndexDict[next];
                    Graph[start].Add(new Edge
                    {
                        KeysToNextNode = starts.Count + nextIndex, 
                        Dist = cur.dist + 1, 
                        KeysNeededMask = nextDoorMask
                    });
                }
                queue.Enqueue((newRow, newColumn, cur.dist + 1, nextDoorMask));
            }
        }
    }

    private static int Dijkstra(List<(int r, int c)> starts, int totalKeysMask, List<Edge>[] graph)
    {
        var queue = new PriorityQueue<State, int>();
        var bestDict = new Dictionary<string, int>();
        var startState = new State { Pos = starts.Select((_, i) => i).ToArray(), Keys = 0 };
        var startKey = startState.Encode();
        bestDict[startKey] = 0;
        queue.Enqueue(startState, 0);
        
        while (queue.Count > 0)
        {
            queue.TryDequeue(out State state, out var distSoFar);
            var stateEncodedKey = state.Encode();
            if (distSoFar != bestDict[stateEncodedKey]) 
                continue;
            if (state.Keys == totalKeysMask) 
                return distSoFar;

            for (var i = 0; i < 4; ++i)
            {
                var fromNode = state.Pos[i];
                foreach (var edge in graph[fromNode])
                {
                    var keyBit = 1 << (edge.KeysToNextNode - starts.Count);
                    if ((state.Keys & keyBit) != 0) 
                        continue; 
                    if ((edge.KeysNeededMask & state.Keys) != edge.KeysNeededMask) 
                        continue;
                    var newPos = (int[])state.Pos.Clone();
                    newPos[i] = edge.KeysToNextNode;
                    var newKeys = state.Keys | keyBit;
                    var newState = new State { Pos = newPos, Keys = newKeys };
                    var newStateKey = newState.Encode();
                    var newDist = distSoFar + edge.Dist;
                    if (bestDict.TryGetValue(newStateKey, out var value) && newDist >= value) 
                        continue;
                    bestDict[newStateKey] = newDist;
                    queue.Enqueue(newState, newDist);
                }
            }
        }

        return -1;
    }

    public static void Main()
    {
        var data = GetInput();
        int result = Solve(data);
        
        if (result == -1)
        {
            Console.WriteLine("No solution found");
        }
        else
        {
            Console.WriteLine(result);
        }
    }
}
