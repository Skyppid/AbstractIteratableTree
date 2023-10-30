using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using TreeIterator;
using TreeIterator.TestApp;
// ReSharper disable RedundantAssignment
#pragma warning disable SYSLIB0011

bool iterate = true;
TreeEnumerationMode enumerationMode = TreeEnumerationMode.DepthFirst;

EnterSelection:
Console.Clear();
Write("Please choose what you want to do:");
Write("1 - Mirror directory in tree, iterate it (timing), serialize " +
      "tree binary (legacy + custom), as XML and dump it as text.");
Write("2 - Deserialize from legacy binary file");
Write("3 - Deserialize from custom binary file");
Write("4 - Deserialize from XML");
Write($"5 - Change deserialization iteration on/off [Current: {iterate}]");
Write($"6 - Change enumeration mode Depth/Breadth [Current: {enumerationMode} - Color " +
      "Code: " + (enumerationMode == TreeEnumerationMode.DepthFirst ? "Cyan" : "Green") + "]");
Write("7 - Exit");

if (!int.TryParse(Console.ReadLine(), out var selection))
    goto EnterSelection;

Stopwatch stopwatch = new();

if (selection == 1)
{
    EnterPath:
    Write("Enter any directory which should be built as tree (or enter for the test directory [recommended]:");
    string? rootDir = Console.ReadLine();
    if (string.IsNullOrEmpty(rootDir))
    {
        rootDir = Path.Combine(Environment.CurrentDirectory, "Test Folder");
    }

    DirectoryInfo root;
    try
    {
        root = new(rootDir);
        if (!root.Exists) throw new();
    }
    catch
    {
        Write("Yeah sure thing, but that ain't no valid directory. Once again...", ConsoleColor.Red);
        goto EnterPath;
    }

    string output = Path.Combine(Environment.CurrentDirectory, "fsTree.txt");
    Write($"A tree of the file system structure will be built now. " +
          $"The output will be placed inside '{output}'.", ConsoleColor.Cyan);
    Write("Depending on your directory this might take a while. Don't close the application! Stuff is happening right now.",
        ConsoleColor.Yellow);

    // Build tree
    FileSystemTreeBranch rootBranch = new(root.Name, true);
    FileSystemTree tree = new(rootBranch);

    BuildTree(root, rootBranch);

    Write("The tree is built up. Press key to start iteration. WARNING: No real performance test!", ConsoleColor.Yellow);
    Console.ReadLine();

    Console.ForegroundColor = ConsoleColor.Magenta;
    tree.EnumerationMode = TreeEnumerationMode.BreadthFirst;

    // Cold run-through

    stopwatch.Start();
    foreach (var _ in tree)
    {
    }

    stopwatch.Stop();
    Write($"Breadth-first cold run-through: {stopwatch.Elapsed.TotalMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds} s).",
        ConsoleColor.Red);

    // Warm run-through
    stopwatch.Restart();
    foreach (var _ in tree)
    {
    }

    stopwatch.Stop();
    Write($"Breadth-first warm run-through: {stopwatch.Elapsed.TotalMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds} s).",
        ConsoleColor.Red);

    // Cold run-through
    tree.EnumerationMode = TreeEnumerationMode.DepthFirst;
    stopwatch.Restart();
    foreach (var _ in tree)
    {
    }

    stopwatch.Stop();
    Write($"Depth-first cold run-through: {stopwatch.Elapsed.TotalMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds} s).",
        ConsoleColor.Red);

    // Warm run-through
    stopwatch.Restart();
    foreach (var _ in tree)
    {
    }

    stopwatch.Stop();
    Write($"Depth-first warm run-through: {stopwatch.Elapsed.TotalMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds} s).",
        ConsoleColor.Red);

    // Dump
    stopwatch.Restart();
    tree.Dump(output);
    stopwatch.Stop();

    Performance("Text dump write down", stopwatch.Elapsed);

    // Serialize
    BinaryFormatter formatter = new();
    using (FileStream fs = new(Path.Combine(Environment.CurrentDirectory, "tree.legacybin"), FileMode.OpenOrCreate))
    {
        stopwatch.Restart();
        formatter.Serialize(fs, tree);
        stopwatch.Stop();
    }

    Performance("Legacy binary write down", stopwatch.Elapsed);

    stopwatch.Restart();
    tree.WriteXml(Path.Combine(Environment.CurrentDirectory, "tree.xml"));
    stopwatch.Stop();

    Performance("XML write down", stopwatch.Elapsed);

    stopwatch.Restart();
    tree.WriteBinary(Path.Combine(Environment.CurrentDirectory, "tree.bin"));
    stopwatch.Stop();

    Performance("Binary write down", stopwatch.Elapsed);

    Console.ReadLine();
    Process.Start(Environment.CurrentDirectory);
}
else if (selection == 2)
{
    string file = Path.Combine(Environment.CurrentDirectory, "tree.legacybin");
    BinaryFormatter formatter = new();
    try
    {
        FileSystemTree fsTree;
        using (FileStream fs = new(file, FileMode.Open))
        {
            // Cold run-through
            stopwatch.Start();
            fsTree = (FileSystemTree)formatter.Deserialize(fs);
            stopwatch.Stop();

            Performance("Legacy binary deserialization cold run-through", stopwatch.Elapsed);

            // Warm run-through
            fs.Seek(0, SeekOrigin.Begin);
            stopwatch.Restart();
            fsTree = (FileSystemTree)formatter.Deserialize(fs);
            stopwatch.Stop();

            Performance("Legacy binary deserialization warm run-through", stopwatch.Elapsed);
        }

        if (iterate)
        {
            Console.ForegroundColor = enumerationMode == TreeEnumerationMode.DepthFirst
                ? ConsoleColor.Cyan
                : ConsoleColor.Green;
            fsTree.EnumerationMode = enumerationMode;
            foreach (var branch in fsTree)
            {
                Console.WriteLine(branch);
            }
        }

        Write(Environment.NewLine);
    }
    catch (Exception ex)
    {
        Write("Error parsing legacy binary file:", ConsoleColor.Red);
        Write(ex.Message + ex.StackTrace, ConsoleColor.Red);
    }

    Write(Environment.NewLine);
    Console.Read();
}
else if (selection == 3)
{
    string file = Path.Combine(Environment.CurrentDirectory, "tree.bin");
    try
    {
        // Cold run-through
        stopwatch.Start();
        FileSystemTree fsTree = Tree.ParseBinary<FileSystemTree>(file);
        stopwatch.Stop();

        Performance("Binary deserialization cold run-through", stopwatch.Elapsed);

        // Warm run-through
        stopwatch.Restart();
        fsTree = (FileSystemTree)Tree<FileSystemTreeBranch>.ParseBinary(file);
        stopwatch.Stop();

        Performance("Binary deserialization warm run-through", stopwatch.Elapsed);

        if (iterate)
        {
            Console.ForegroundColor = enumerationMode == TreeEnumerationMode.DepthFirst
                ? ConsoleColor.Cyan
                : ConsoleColor.Green;
            fsTree.EnumerationMode = enumerationMode;
            foreach (var branch in fsTree)
            {
                Console.WriteLine(branch);
            }
        }

        Write(Environment.NewLine);
    }
    catch (Exception ex)
    {
        Write("Error parsing binary file:", ConsoleColor.Red);
        Write(ex.Message + ex.StackTrace, ConsoleColor.Red);
    }

    Write(Environment.NewLine);
    Console.Read();
}
else if (selection == 4)
{
    string file = Path.Combine(Environment.CurrentDirectory, "tree.xml");
    try
    {
        // Cold run-through
        stopwatch.Start();
        FileSystemTree fsTree = (FileSystemTree)Tree.ParseXml(file);
        stopwatch.Stop();

        Performance("XML deserialization cold run-through", stopwatch.Elapsed);

        // Warm run-through
        stopwatch.Restart();
        fsTree = (FileSystemTree)Tree.ParseXml(file);
        stopwatch.Stop();

        Performance("XML deserialization warm run-through", stopwatch.Elapsed);

        if (iterate)
        {
            Console.ForegroundColor = enumerationMode == TreeEnumerationMode.DepthFirst
                ? ConsoleColor.Cyan
                : ConsoleColor.Green;
            fsTree.EnumerationMode = enumerationMode;
            foreach (var branch in fsTree)
            {
                Console.WriteLine(branch);
            }
        }

        Write(Environment.NewLine);
    }
    catch (Exception ex)
    {
        Write("Error parsing XML file:", ConsoleColor.Red);
        Write(ex.Message + ex.StackTrace, ConsoleColor.Red);
    }

    Write(Environment.NewLine);
    Console.Read();
}
else if (selection == 5)
{
    iterate = !iterate;
}
else if (selection == 6)
{
    enumerationMode = enumerationMode == TreeEnumerationMode.DepthFirst
        ? TreeEnumerationMode.BreadthFirst
        : TreeEnumerationMode.DepthFirst;
}
else if (selection == 7) return;

goto EnterSelection;


static void BuildTree(DirectoryInfo current, FileSystemTreeBranch branch)
{
    // Scans the file system structure and builds up the tree
    // The try/catch is necessary for system files as these would fire exceptions
    try
    {
        foreach (var file in current.GetFiles())
            branch.AddBranch(new FileSystemTreeBranch(file.Name, false));
        foreach (var dir in current.GetDirectories())
        {
            FileSystemTreeBranch next = new(dir.Name, true);
            BuildTree(dir, next);
            branch.AddBranch(next);
        }
    }
    catch (UnauthorizedAccessException)
    {
    }
}

static void Write(string message, ConsoleColor color = ConsoleColor.White)
{
    Console.ForegroundColor = color;
    Console.WriteLine(message);
}

static void Performance(string name, TimeSpan timing)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine($"[PERFORMANCE] {name}: {timing.TotalMilliseconds} ms ({timing.TotalSeconds} s)");
}