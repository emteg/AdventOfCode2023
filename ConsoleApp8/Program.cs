using Util;

internal class Program
{
    internal const char Left = 'L';
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        TextReader sampleInput = new StringReader(@"RL

AAA = (BBB, CCC)
BBB = (DDD, EEE)
CCC = (ZZZ, GGG)
DDD = (DDD, DDD)
EEE = (EEE, EEE)
GGG = (GGG, GGG)
ZZZ = (ZZZ, ZZZ)");

        TextReader sampleInput2 = new StringReader(@"LLR

AAA = (BBB, BBB)
BBB = (AAA, ZZZ)
ZZZ = (ZZZ, ZZZ)");

        TextReader input = File.OpenText("input.txt");
        Part1(sampleInput);
        Part1(sampleInput2);
        Part1(input);

        TextReader sampleInput3 = new StringReader(@"LR

11A = (11B, XXX)
11B = (XXX, 11Z)
11Z = (11B, XXX)
22A = (22B, XXX)
22B = (22C, 22C)
22C = (22Z, 22Z)
22Z = (22B, 22B)
XXX = (XXX, XXX)");
        
        input = File.OpenText("input.txt");
        Part2(sampleInput3);
        Part2(input);
    }

    private static void Part2(TextReader input)
    {
        char[] instructions = input.ReadLine()!.ToCharArray();
        
        List<KeyValuePair<string, NodeConnections>> parsed = ReadNodes(input).ToList();
        Dictionary<string, NodeConnections> nodes = new(parsed);
        string[] first2NodesEndingInA = parsed
            .Select(it => it.Key)
            .Where(it => it.EndsWith('A'))
            .Take(2)
            .ToArray();

        string name1 = first2NodesEndingInA[0];
        string name2 = first2NodesEndingInA[1];
        uint numberOfSteps = 0;
        int instructionPointer = 0;

        while (!(name1.EndsWith('Z') && name2.EndsWith('Z')))
        {
            char instruction = instructions[instructionPointer];
            name1 = nodes[name1].GoTo(instruction);
            name2 = nodes[name2].GoTo(instruction);
            numberOfSteps++;
            IncInstructionPointer(ref instructionPointer, instructions.Length);
        }
        
        Console.WriteLine(numberOfSteps);
    }

    private static void Part1(TextReader input)
    {
        char[] instructions = input.ReadLine()!.ToCharArray();
        Dictionary<string, NodeConnections> nodes = new(ReadNodes(input));
        
        string name = "AAA";
        uint numberOfSteps = 0;
        int instructionPointer = 0;

        while (name != "ZZZ")
        {
            name = nodes[name].GoTo(instructions[instructionPointer]);
            numberOfSteps++;
            IncInstructionPointer(ref instructionPointer, instructions.Length);
        }
        
        Console.WriteLine(numberOfSteps);
    }

    private static IEnumerable<KeyValuePair<string, NodeConnections>> ReadNodes(TextReader input)
    {
        return input
            .EnumerateLines()
            .Where(line => line.Length > 0)
            .Select(ToNode);
    }

    private static KeyValuePair<string, NodeConnections> ToNode(string line)
    {
        string[] bits = line.Split(" = ");
        string name = bits[0];
        string[] connections = bits[1].Substring(1, 8).Split(", ");

        return new KeyValuePair<string, NodeConnections>(name, new NodeConnections(connections[0], connections[1]));
    }

    private readonly struct NodeConnections
    {
        public readonly string Left; 
        public readonly string Right;

        public NodeConnections(string left, string right)
        {
            Left = left;
            Right = right;
        }

        public string GoTo(char instruction) 
            => instruction == Program.Left
                ? Left
                : Right;
    }

    private static void IncInstructionPointer(ref int instructionPointer, int length)
    {
        instructionPointer++;
        if (instructionPointer >= length)
            instructionPointer = 0;
    }
}