// See https://aka.ms/new-console-template for more information

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        string sampleInput = "rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7";
        string input = File.ReadAllText("input.txt");
        
        Console.WriteLine(Part1("HASH"));
        Console.WriteLine(Part1(sampleInput));
        Console.WriteLine(Part1(input));

        Console.WriteLine(Part2(sampleInput));
        Console.WriteLine(Part2(input));
    }

    private static int Part2(string input)
    {
        List<(int key, Instruction inst)> instructions = input
            .Split(',')
            .Select(Instruction.FromString)
            .Select(inst => (Hash(inst.Label), inst))
            .ToList();
        Dictionary<int, List<Instruction>> boxesAndLenses = InitializeBoxes();
        ApplyInstructionsToBoxes(instructions, boxesAndLenses);
        return boxesAndLenses.Select(BoxFocusingPower).Sum();
    }

    private static int BoxFocusingPower(KeyValuePair<int, List<Instruction>> box)
    {
        int result = 0;
        int lensNumber = 1;
        for (var i = 0; i < box.Value.Count; i++)
        {
            result += (box.Key + 1) * lensNumber * box.Value[i].FocalLength;
            lensNumber++;
        }
        return result;
    }

    private static Dictionary<int, List<Instruction>> InitializeBoxes()
    {
        Dictionary<int, List<Instruction>> boxesAndLenses = new();
        for (int i = 0; i < 256; i++) 
            boxesAndLenses.Add(i, new List<Instruction>());
        return boxesAndLenses;
    }

    private static void ApplyInstructionsToBoxes(List<(int key, Instruction inst)> instructions, Dictionary<int, List<Instruction>> boxesAndLenses)
    {
        foreach ((int key, Instruction instruction) in instructions)
        {
            List<Instruction> box = boxesAndLenses[key];
            if (instruction.Operation is Operation.InsertLens)
            {
                if (box.Any(it => it.Label == instruction.Label)) // replace
                {
                    Instruction toReplace = box.First(it => it.Label == instruction.Label);
                    box.Insert(box.IndexOf(toReplace), instruction);
                    box.Remove(toReplace);
                    continue;
                }
                
                // add
                box.Add(instruction);
                continue;   
            }
            
            // remove lens if present
            foreach (Instruction lens in box)
            {
                if (lens.Label == instruction.Label)
                {
                    box.Remove(lens);
                    break;
                }
            }
        }
    }

    private enum Operation
    {
        RemoveLens,
        InsertLens
    }

    private record Instruction(string Label, Operation Operation, byte FocalLength = 0)
    {
        public static Instruction FromString(string s)
        {
            int operationIndex = s.IndexOf(s.Contains('=') ? '=' : '-');
            
            string label = s.Substring(0, operationIndex);
            Program.Operation operation = s[operationIndex] switch
            {
                '-' => Operation.RemoveLens,
                '=' => Operation.InsertLens,
                _ => throw new InvalidOperationException()
            };
            byte focalLength = operation is Operation.InsertLens
                ? byte.Parse(s[(operationIndex + 1)..])
                : (byte)0;

            return new Instruction(label, operation, focalLength);
        }

        public string LensLabel => $"{Label} {FocalLength}";
    }

    private static int Part1(string initializationSequence) 
        => initializationSequence
            .Split(',')
            .Select(Hash)
            .Sum();

    private static int Hash(string input)
    {
        int result = 0;

        foreach (char c in input)
        {
            result += c;
            result *= 17;
            result = result % 256;
        }
        
        return result;
    }
}