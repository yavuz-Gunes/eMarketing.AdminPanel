using System.Diagnostics;

string rootDirectory = Directory.GetCurrentDirectory();
string apiProject = Path.Combine(rootDirectory, "eMarketing.Api", "eMarketing.Api.csproj");

if (!File.Exists(apiProject))
{
    Console.Error.WriteLine("API project was not found. Run this command from the eMarketing repository root.");
    return 1;
}

string forwardedArgs = args.Length == 0
    ? string.Empty
    : " " + string.Join(" ", args.Select(QuoteArgument));

var startInfo = new ProcessStartInfo
{
    FileName = "dotnet",
    Arguments = "run --project " + QuoteArgument(apiProject) + " --urls http://localhost:5088" + forwardedArgs,
    UseShellExecute = false
};

using Process? process = Process.Start(startInfo);
if (process == null)
{
    Console.Error.WriteLine("API process could not be started.");
    return 1;
}

process.WaitForExit();
return process.ExitCode;

static string QuoteArgument(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return "\"\"";

    return value.Contains(' ') ? "\"" + value.Replace("\"", "\\\"") + "\"" : value;
}
