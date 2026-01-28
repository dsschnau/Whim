using BenchmarkDotNet.Attributes;
using System.IO;
using System;
using Whim;
using Microsoft.VSDiagnostics;

[CPUUsageDiagnoser]
public class ConfigLoaderBenchmark
{
    private ConfigLoader _configLoader;
    [GlobalSetup]
    public void Setup()
    {
        _configLoader = new ConfigLoader(new TestFileManager());
    }

    [Benchmark]
    public void LoadConfigBenchmark()
    {
        var doConfig = _configLoader.LoadConfig();
    // Do not invoke the delegate to avoid needing a real IContext implementation.
    // The aim is to measure the cost of LoadConfig (including script evaluation).
    }

    private class TestFileManager : IFileManager
    {
        public string WhimDir => "C:\\temp\\whim";
        public string LogsDir => "C:\\temp\\whim\\logs";
        public string SavedStateDir => "C:\\temp\\whim\\state";

        public void EnsureDirExists(string dir)
        {
        }

        public bool FileExists(string filePath) => true;
        public string GetWhimFileDir(string fileName) => fileName;
        public string GetWhimFileLogsDir(string fileName) => fileName;
        public Stream OpenRead(string filePath) => throw new NotImplementedException();
        public string ReadAllText(string filePath) => "(Whim.DoConfig)(context => { });";
        public void WriteAllText(string filePath, string contents)
        {
        }

        public void DeleteFile(string filePath)
        {
        }
    }
}