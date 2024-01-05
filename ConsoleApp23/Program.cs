// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        string sampleInput = @"#.#####################
#.......#########...###
#######.#########.#.###
###.....#.>.>.###.#.###
###v#####.#v#.###.#.###
###.>...#.#.#.....#...#
###v###.#.#.#########.#
###...#.#.#.......#...#
#####.#.#.#######.#.###
#.....#.#.#.......#...#
#.#####.#.#.#########v#
#.#...#...#...###...>.#
#.#.#v#######v###.###v#
#...#.>.#...>.>.#.###.#
#####v#.#.###v#.#.###.#
#.....#...#...#.#.#...#
#.#########.###.#.#.###
#...###...#...#...#.###
###.###.#.###v#####v###
#...#...#.#.>.>.#.>.###
#.###.###.#.###.#.#v###
#.....###...###...#...#
#####################.#";

        //uint longestPath = Part1(new StringReader(sampleInput).EnumerateLines().ToArray(), true);
        uint longestPath = Part1(File.OpenText("input.txt").EnumerateLines().ToArray(), true);
        
        Console.WriteLine($"Longest path from start to goal is {longestPath} steps long.");
    }

    private static uint Part1(string[] lines, bool ignoreOneWay)
    {
        (Node start, Node goal) = ReadGrid(lines, ignoreOneWay);

        if (ignoreOneWay)
        {
            // we change the connection from start to the  node after that to be one way
            start.Connections[0].node.DisconnectFrom(start);

            // we change the connections coming into the node before the goal to one way
            // because there is no valid path that does not go straight to goal from here
            Node beforeGoal = goal.Connections.Select(it => it.node).First();
            var nodesAwayFromBeforeGoal = beforeGoal.Connections
                .Where(it => it.node != goal)
                .Select(it => it.node)
                .ToArray();
            foreach (Node node in nodesAwayFromBeforeGoal)
            {
                beforeGoal.DisconnectFrom(node);
                node.Required = true; // the nodes leading into the node before the goal must both be passed
            }
            
            // paths that pass through beforeGoal must have passed through at least 2 (=all) required nodes
            beforeGoal.MinimumRequiredNodes = 2;
            
            // finally we also make the connection from before goal to goal one way
            goal.DisconnectFrom(beforeGoal);
        }

        HashSet<Path> paths = new() { new Path(start) };
        Path? longestAcceptable = null;
        bool pathCreatedOrExtended = true;
        List<(Action action, Path path)> actions = new();
        while (pathCreatedOrExtended)
        {
            pathCreatedOrExtended = false;
            
            Console.WriteLine($"There currently are {paths.Count} paths ({actions.Count(it => it.action is Action.Delete)} removed, {actions.Count(it => it.action is Action.Add)} added).");
            Console.WriteLine($"The longest path to goal has a length of {longestAcceptable?.Length}\n  via {string.Join(", ", longestAcceptable?.Nodes.Select(n => $"{n.Number} ({n.X},{n.Y})") ?? Array.Empty<string>())}");
            Console.WriteLine("Performing cleanup...");
            
            Console.WriteLine("Deleting paths...");
            foreach (Path p in actions.Where(it => it.action is Action.Delete).Select(it => it.path))
                paths.Remove(p);
            
            Console.WriteLine("Adding new paths...");
            foreach (Path path in actions.Where(it => it.action is Action.Add).Select(it => it.path!)) 
                paths.Add(path);
            
            actions.Clear();

            int i = 0;
            foreach (Path path in paths)
            {
                if (i % 30000 == 0)
                    Console.WriteLine($"Exploring path #{i} of {paths.Count}...");

                List<(Action action, Path path)> newActions = ExplorePath(path, goal, longestAcceptable).ToList();

                pathCreatedOrExtended |= newActions.Any(it => it.action is not Action.Delete);
                
                foreach (Path newLongest in newActions.Where(it => it.action is Action.NewLongest).Select(it => it.path))
                {
                    if (longestAcceptable is null || longestAcceptable.Length < newLongest.Length) 
                        longestAcceptable = newLongest;
                }
                
                actions.AddRange(newActions.Where(it => it.action is not Action.NewLongest));
                i++;
            }
        }

        return longestAcceptable?.Length ?? 0;
    }

    private static IEnumerable<(Action, Path)> ExplorePath(Path path, Node goal, Path? longestAcceptable)
    {
        uint pathsExtendedOrSplit = 0;
        int originalPathNodeCount = path.Nodes.Count;
        uint originalPathLength = path.Length;
        Node originalEndNode = path.End;
        int originalRequiredNodes = path.RequiredNodes;
        bool firstNode = true;
        bool newLongestFound = false;
        
        for (int j = 0; j < originalEndNode.Connections.Count; j++)
        {
            if (!path.CanAppend(originalEndNode.Connections[j].node))
                continue;
            
            Node node = originalEndNode.Connections[j].node;
            uint cost = originalEndNode.Connections[j].cost;
            
            // here we enforce that only paths are created that go through all required nodes before
            // going to the goal
            if (originalEndNode.Required && originalRequiredNodes < node.MinimumRequiredNodes)
                continue;

            Path newPath;
            if (firstNode)
            {
                path.Append(node, cost);
                firstNode = false;
                newPath = path;
                yield return (Action.Extended, path);
            }
            else
            {
                newPath = path.AppendAndSplit(node, originalPathNodeCount, originalPathLength + cost);
                yield return (Action.Add, newPath);
            }
            
            pathsExtendedOrSplit++;
            if (newPath.EndsAt(goal))
            {
                if (longestAcceptable is null || longestAcceptable.Length < newPath.Length)
                {
                    longestAcceptable = newPath;
                    newLongestFound = true;
                    Console.WriteLine($"Found new longest path to goal with a length of {newPath.Length}\n  via {string.Join(", ", path.Nodes.Select(n => $"{n.Number} ({n.X},{n.Y})"))}");
                }
            }
        }

        if (pathsExtendedOrSplit == 0)
            yield return (Action.Delete, path);
        
        if (newLongestFound)
            yield return (Action.NewLongest, longestAcceptable!);

    }

    private static (Node start, Node goal) ReadGrid(string[] lines, bool ignoreOneWay)
    {
        uint nodeNumber = 1;
        Node start = new((byte)lines[0].IndexOf('.'), 0, nodeNumber++);
        Node goal = new((byte)lines[^1].IndexOf('.'), (byte)(lines.Length - 1), nodeNumber++);

        Dictionary<(byte x, byte y), Node> nodes = new();
        nodes.Add((start.X, start.Y), start);
        nodes.Add((goal.X, goal.Y), goal);
        
        Queue<(Node node, List<(byte x, byte y)> startingPoints)> queue = new();
        queue.Enqueue((start, new List<(byte x, byte y)>{(start.X, (byte)(start.Y + 1))}));

        while (queue.TryDequeue(out (Node node, List<(byte x, byte y)> startingPoints) last))
        {
            Console.WriteLine($"Exploring paths from node N({last.node.X},{last.node.Y})...");
            if (last.node == goal)
            {
                Console.WriteLine("  that's the goal.");
                continue;
            }
            foreach ((byte startX, byte startY) in last.startingPoints)
            {
                Console.WriteLine($"  ...from ({startX},{startY})...");
                byte x = startX;
                byte y = startY;
                byte lastX = last.node.X;
                byte lastY = last.node.Y;
                uint cost = 1;
                bool isOneWay = lines[startY][startX] != '.';
                
                while (true)
                {
                    (bool isJunction, List<(char c, byte x, byte y)> neighbors) = NeighborsOf(x, y, lastX, lastY, lines, ignoreOneWay);
                    if (neighbors.Count == 1 && !isJunction)
                    {
                        lastX = x;
                        lastY = y;
                        cost++;
                        x = neighbors[0].x;
                        y = neighbors[0].y;
                        if (neighbors[0].c != '.' && !isOneWay)
                            isOneWay = true;
                        continue;
                    }

                    Node newNode;
                    if (nodes.ContainsKey((x, y)))
                    {
                        newNode = nodes[(x, y)];
                        Console.WriteLine($"  Found already existing node at ({x},{y}).");
                    }
                    else
                    {
                        newNode = new Node(x, y, nodeNumber++);
                        nodes.Add((x, y), newNode);
                        Console.WriteLine($"  Found new node at ({x},{y}).");
                        queue.Enqueue((newNode, neighbors.Select(it => (it.x, it.y)).ToList()));
                    }

                    char dir = ignoreOneWay 
                        ? '-'
                        : isOneWay 
                            ? '>' 
                            : '-';
                    Console.WriteLine($"  N({last.node.X},{last.node.Y})--{cost}-{dir}N({x},{y}).");
                    last.node.ConnectTo(newNode, cost, !ignoreOneWay && isOneWay);
                    
                    break;
                }
            }
        }
        Console.WriteLine("Exploration completed.");
        
        return (start, goal);
    }

    private static (bool isJunction, List<(char c, byte x, byte y)>) NeighborsOf(
        byte x, byte y, byte fromX, byte fromY, string[] lines, bool ignoreOneWay)
    {
        List<(char c, byte x, byte y)> result = new();
        byte neighbors = 0;
        if (x > 0 && fromX != x - 1 && lines[y][x - 1] != '#')
        {
            neighbors++;
            if (lines[y][x - 1] != '>' || ignoreOneWay)
                result.Add((lines[y][x - 1], (byte)(x - 1), y));
        }

        if (x < lines[0].Length - 1 && fromX != x + 1 && lines[y][x + 1] != '#')
        {
            neighbors++;
            if (lines[y][x + 1] != '<' || ignoreOneWay)
                result.Add((lines[y][x + 1], (byte)(x + 1), y));
        }

        if (y > 0 && fromY != y - 1 && lines[y - 1][x] != '#')
        {
            neighbors++;
            if (lines[y - 1][x] != 'v' || ignoreOneWay)
                result.Add((lines[y - 1][x], x, (byte)(y - 1)));
        }

        if (y < lines.Length - 1 && fromY != y + 1 && lines[y + 1][x] != '#')
        {
            neighbors++;
            result.Add((lines[y + 1][x], x, (byte)(y + 1)));
        }

        return (neighbors > 1, result);
    }
}

internal sealed class Path
{
    public IReadOnlyList<Node> Nodes => nodes;
    public uint Length { get; private set; } = 0;
    public int RequiredNodes => nodes.Count(it => it.Required);

    public Node End => Nodes[^1];

    public Path(Node start)
    {
        nodes.Add(start);
    }

    public bool CanAppend(Node? node)
    {
        if (node is null)
            return false;
        return !nodes.Contains(node);
    }

    public void Append(Node node, uint cost)
    {
        Length += cost;
        nodes.Add(node);
    }

    public Path AppendAndSplit(Node node, int nodesToTake, uint length) 
        => new(this, nodesToTake, node, length);

    public bool EndsAt(Node node) => nodes.Last() == node;

    private Path(Path toBeSplitFrom, int nodesToTake, Node nextNode, uint length)
    {
        Length = length;
        nodes.AddRange(toBeSplitFrom.nodes.Take(nodesToTake));
        nodes.Add(nextNode);
    }

    private readonly List<Node> nodes = new();
}

[DebuggerDisplay("#{Number}: {X}|{Y}")]
internal sealed class Node
{
    public readonly byte X;
    public readonly byte Y;
    public readonly uint Number;
    public bool Required = false;
    public int MinimumRequiredNodes = 0;
    public IReadOnlyList<(Node node, uint cost)> Connections => connections;

    public Node(byte x, byte y, uint number)
    {
        X = x;
        Y = y;
        Number = number;
    }

    public void ConnectTo(Node node, uint cost, bool isOneWay)
    {
        if (IsConnectedTo(node)) 
            return;
        
        connections.Add((node, cost));
        
        if (!isOneWay &&!node.IsConnectedTo(this)) 
            node.connections.Add((this, cost));
    }

    public void DisconnectFrom(Node node)
    {
        (Node node, uint cost) con = connections.First(it => it.node == node);
        connections.Remove(con);
    }

    private bool IsConnectedTo(Node node) => connections.Any(tuple => tuple.node == node);

    private readonly List<(Node node, uint cost)> connections = new();
}

internal enum Action
{
    Add,
    Delete,
    NewLongest,
    Extended
}