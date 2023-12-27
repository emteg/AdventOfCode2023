// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        TextReader sampleInput1 = new StringReader(@"broadcaster -> a, b, c
%a -> b
%b -> c
%c -> inv
&inv -> a");

        //Console.WriteLine(Part1(sampleInput1.EnumerateLines()));

        TextReader sampleInput2 = new StringReader(@"broadcaster -> a
%a -> inv, con
&inv -> b
%b -> con
&con -> output");
        
        //Console.WriteLine(Part1(sampleInput2.EnumerateLines()));
        
        //Console.WriteLine(Part1(File.OpenText("input.txt").EnumerateLines()));

        // resets after 3733 button pushes
        TextReader shiftRegister1 = new StringReader(@"broadcaster -> br
%br -> vn, jz
%jz -> qs
%qs -> vn, ps
%ps -> xm
%xm -> vn, ht
%ht -> pp
%pp -> mq
%mq -> vn, zc
%zc -> zv
%zv -> dc, vn
%dc -> mg, vn
%mg -> vn
&vn -> br, jz, ht, ps, zc, pp, ds");
        
        // resets after 3797 button pushes
        TextReader shiftRegister2 = new StringReader(@"broadcaster -> bh
%bh -> qd, vv
%qd -> xb
%xb -> bl, vv
%bl -> bb
%bb -> nb, vv
%nb -> dv
%dv -> vv, hr
%hr -> dm, vv
%dm -> kg
%kg -> mr, vv
%mr -> vv, pz
%pz -> vv
&vv -> dm, bl, sb, nb, qd, bh");
        
        // resets after 3917 button pushes
        TextReader shiftRegister3 = new StringReader(@"broadcaster -> fg
%fg -> nt, gt
%gt -> jc
%jc -> nt, nk
%nk -> rq, nt
%rq -> ft
%ft -> fh
%fh -> nt, xz
%xz -> dr
%dr -> xd, nt
%xd -> nt, lm
%lm -> nt, qn
%qn -> nt
&nt -> rq, fg, ft, nd, gt, xz");
        
        // resets after 3877 button pushes
        TextReader shiftRegister4 = new StringReader(@"broadcaster -> pj
%pj -> zj, zq
%zj -> cf
%cf -> gr, zq
%gr -> ln
%ln -> rm
%rm -> zq, fs
%fs -> ff
%ff -> cl
%cl -> fp, zq
%fp -> rd, zq
%rd -> nc, zq
%nc -> zq
&zq -> fs, gr, ff, hf, ln, zj, pj
");

        Console.WriteLine(Part2(shiftRegister4.EnumerateLines()));
        
        // the answer to part 2 is the least common multiple of the button pushes required to
        // reset each of the shift registers
    }

    private static ulong Part1(IEnumerable<string> lines)
    {
        lowPulsesSent = 0;
        highPulsesSent = 0;
        
        List<Module> modules = CreateModules(lines);
        ButtonModule button = new(PulseSent);
        button.ConnectTo((ReceivingModule)modules.First(it => it.Name == "broadcaster"));

        Queue<Module> moduleQueue = new();

        for (int i = 0; i < 1000; i++) 
            PushButton(button, moduleQueue);

        return lowPulsesSent * highPulsesSent;
    }
    
    private static uint Part2(IEnumerable<string> lines)
    {
        lowPulsesSent = 0;
        highPulsesSent = 0;
        
        List<Module> modules = CreateModules(lines);
        ButtonModule button = new(PulseSent);
        button.ConnectTo((ReceivingModule)modules.First(it => it.Name == "broadcaster"));

        Queue<Module> moduleQueue = new();

        uint count = 0;
        while (true)
        {
            lowPulsesSentToRx = 0;
            highPulsesSentToRx = 0;
            PushButton(button, moduleQueue);
            count++;

            if (lowPulsesSentToRx == 1)
                break;
        }

        return count;
    }

    private static void PushButton(ButtonModule button, Queue<Module> moduleQueue)
    {
        foreach (Module module in button.Push()) 
            moduleQueue.Enqueue(module);

        while (moduleQueue.TryDequeue(out Module? module))
        {
            foreach (Module mod in module.Update()) 
                moduleQueue.Enqueue(mod);
        }
    }

    private static List<Module> CreateModules(IEnumerable<string> input)
    {
        Dictionary<string, (Module module, string[] outputs)> modules = new();
        foreach (string line in input)
        {
            (Module module, string[] outputs) = CreateModule(line);
            modules.Add(module.Name, (module, outputs));
        }

        Dictionary<string, Module> allModules = modules
            .Select(it => new KeyValuePair<string, Module>(it.Key, it.Value.module))
            .ToDictionary(it => it.Key, it => it.Value);
        
        foreach ((_, (Module module, string[] outputs) value) in modules)
        {
            foreach (string outModuleName in value.outputs)
            {
                if (!allModules.ContainsKey(outModuleName))
                    allModules.Add(outModuleName, new OutputModule(outModuleName, PulseSent));
                    
                value.module.ConnectTo((ReceivingModule)allModules[outModuleName]);
            }
        }

        return allModules.Values.ToList();
    }
    
    private static (Module module, string[] outputs) CreateModule(string line)
    {
        string[] parts = line.Split(" -> ");
        string name = parts[0];
        string[] outputs = parts[1].Split(", ");

        if (name == "broadcaster")
            return (new BroadcastModule(PulseSent), outputs);

        if (name.StartsWith('%'))
            return (new FlipFlopModule(name.Substring(1), PulseSent), outputs);
        
        if (name.StartsWith('&'))
            return (new ConjunctionModule(name.Substring(1), PulseSent), outputs);

        throw new InvalidOperationException();
    }

    private static uint lowPulsesSent = 0;
    private static uint highPulsesSent = 0;
    private static uint lowPulsesSentToRx = 0;
    private static uint highPulsesSentToRx = 0;

    private static void PulseSent(string senderModuleName, bool isHigh, string receivingModuleName)
    {
        //Console.WriteLine($"{senderModuleName} -{(isHigh ? "high" : "low")}-> {receivingModuleName}");

        if (isHigh)
            highPulsesSent++;
        else
            lowPulsesSent++;

        if (receivingModuleName == "hf")
        {
            if (isHigh)
                highPulsesSentToRx++;
            else
                lowPulsesSentToRx++;
        }
    }
}

internal abstract class Module
{
    public readonly string Name;
    public IReadOnlyList<ReceivingModule> Outputs => outputs;
    
    protected Module(string name, Action<string, bool, string> pulseSent)
    {
        Name = name;
        PulseSent = pulseSent;
    }

    /// <summary>
    /// Adds the given <paramref name="other"/> Module to its outputs and informs the other module that this
    /// module is going to be one of its inputs.
    /// </summary>
    public void ConnectTo(ReceivingModule other)
    {
        outputs.Add(other);
        other.ConnectFrom(this);
    }
    
    /// <summary> Updates its internal state and returns all modules that pulses were sent to (if any).</summary>
    public abstract IEnumerable<Module> Update();

    public override string ToString() => Name;

    protected IEnumerable<Module> SendPulseToAllOutputs(bool isHigh)
    {
        foreach (ReceivingModule output in outputs)
        {
            output.Receive(this, isHigh);
            PulseSent(Name, isHigh, output.Name);
            yield return output;
        }
    }

    protected List<ReceivingModule> outputs = new();
    
    /// <summary>
    /// A callback that is called whenever a pulse is sent. Parameters:<br/>
    /// Name of the sending module<br/>
    /// Whether a high pulse was sent<br/>
    /// Name of the receiving module
    /// </summary>
    protected readonly Action<string, bool, string> PulseSent;
}

internal sealed class ButtonModule : Module {
    public ButtonModule(Action<string, bool, string> pulseSent) : base("button", pulseSent) { }

    /// <summary>Alias for <see cref="Update"/></summary>
    public IEnumerable<Module> Push() => Update();

    /// <summary> Sends a low pulse to all outputs.</summary>
    public override IEnumerable<Module> Update() => SendPulseToAllOutputs(false);
}

internal abstract class ReceivingModule : Module
{
    public virtual void ConnectFrom(Module other) {}
    
    /// <summary>
    /// Adds the given pulse type <paramref name="isHigh"/> to the queue of pulses received, noting which module
    /// was the <paramref name="sender"/> of the pulse.
    /// </summary>
    public void Receive(Module sender, bool isHigh)
    {
        receivedPulses.Enqueue((sender, isHigh));
    }

    protected ReceivingModule(string name, Action<string, bool, string> pulseSent) : base(name, pulseSent) { }
    
    protected Queue<(Module sender, bool isHigh)> receivedPulses = new();
}

internal sealed class FlipFlopModule : ReceivingModule
{
    public FlipFlopModule(string name, Action<string, bool, string> pulseSent) : base(name, pulseSent) { }

    public override IEnumerable<Module> Update()
    {
        while (receivedPulses.TryDequeue(out (Module from, bool isHigh) pulse))
        {
            if (pulse.isHigh)
                continue;

            bool wasOn = isOn;
            isOn = !isOn;
            bool sendHighPulse = !wasOn && isOn;
            
            foreach (Module sent in SendPulseToAllOutputs(sendHighPulse))
                yield return sent;
        }
    }
    
    public override string ToString() => $"{Name} ({(isOn ? "on" : "off")})";

    private bool isOn = false;
}

internal sealed class ConjunctionModule : ReceivingModule
{
    public ConjunctionModule(string name, Action<string, bool, string> pulseSent) : base(name, pulseSent) { }

    public override void ConnectFrom(Module other)
    {
        lastPulseTypeReceivedFrom.Add(other, false);
    }

    public override IEnumerable<Module> Update()
    {
        while (receivedPulses.TryDequeue(out (Module from, bool isHigh) pulse))
        {
            lastPulseTypeReceivedFrom[pulse.from] = pulse.isHigh;
        }

        bool sendHighPulse = !lastPulseTypeReceivedFrom.All(pair => pair.Value);

        foreach (Module sent in SendPulseToAllOutputs(sendHighPulse))
            yield return sent;
    }
    
    public override string ToString()
    {
        return $"{Name} ({string.Join(", ", lastPulseTypeReceivedFrom.Select(it => $"{it.Key.Name}: {it.Value}"))})";
    }

    private readonly Dictionary<Module, bool> lastPulseTypeReceivedFrom = new();
}

internal sealed class BroadcastModule : ReceivingModule
{
    public BroadcastModule(Action<string, bool, string> pulseSent) : base("broadcaster", pulseSent) { }

    public override IEnumerable<Module> Update()
    {
        while (receivedPulses.TryDequeue(out (Module from, bool isHigh) pulse))
            foreach (Module sent in SendPulseToAllOutputs(pulse.isHigh))
                yield return sent;
    }
}

internal sealed class OutputModule : ReceivingModule
{
    public bool LastPulseReceivedWasHigh { get; private set; }
    
    public OutputModule(string name, Action<string, bool, string> pulseSent) : base(name, pulseSent) { }

    public override IEnumerable<Module> Update()
    {
        while (receivedPulses.TryDequeue(out (Module from, bool isHigh) pulse)) 
            LastPulseReceivedWasHigh = pulse.isHigh;
        
        yield break;
    }
}