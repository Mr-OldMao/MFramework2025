using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FlatcCompilerService
{
    public bool Compile(string flatcPath, string fbsPath, string jsonPath, string outputPath,string dataOutPutPath, Action<string> consoleOutput)
    {
        flatcPath = FindFlatcCompiler(flatcPath);
        if (string.IsNullOrEmpty(flatcPath))
        {
            consoleOutput?.Invoke("错误: 未找到flatc编译器");
            return false;
        }

        try
        {
            // 第一步：生成C#代码
            string csArguments = $"--csharp -o \"{outputPath}\" \"{fbsPath}\"";
            consoleOutput?.Invoke($"生成C#代码: {flatcPath} {csArguments}");
            ExecuteCommand(flatcPath, csArguments, consoleOutput);

            // 第二步：生成二进制文件
            string binArguments = $"-b -o \"{dataOutPutPath}\" \"{fbsPath}\" \"{jsonPath}\"";
            consoleOutput?.Invoke($"生成二进制文件: {flatcPath} {binArguments}");
            ExecuteCommand(flatcPath, binArguments, consoleOutput);

            // 第三步：创建.bytes文件
            CreateBytesFile(jsonPath, dataOutPutPath, consoleOutput);

            consoleOutput?.Invoke("编译完成!");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"编译失败: {e.Message}");

            consoleOutput?.Invoke($"编译失败: {e.Message}");
            return false;
        }
    }

    private void CreateBytesFile(string jsonPath, string outputPath, Action<string> consoleOutput)
    {
        try
        {
            string baseName = Path.GetFileNameWithoutExtension(jsonPath);
            string binPath = Path.Combine(outputPath, $"{baseName}.bin");
            string bytesPath = Path.Combine(outputPath, $"{baseName}.bytes");

            if (File.Exists(binPath))
            {
                // 直接将.bin文件复制为.bytes文件
                File.Copy(binPath, bytesPath, true);
                //consoleOutput?.Invoke($"✓ 生成.bytes文件: {bytesPath}");
            }
            else
            {
                consoleOutput?.Invoke($"✗ 未找到.bin文件，无法创建.bytes文件: {binPath}");
            }
        }
        catch (Exception e)
        {
            consoleOutput?.Invoke($"创建.bytes文件失败: {e.Message}");
        }
    }

    public string FindFlatcCompiler(string flatcPath = null)
    {
        if (!string.IsNullOrEmpty(flatcPath) && File.Exists(flatcPath))
        {
            return flatcPath;
        }

        string[] possiblePaths = {
            "flatc",
            "/usr/local/bin/flatc",
            "C:/flatc/flatc.exe",
            Path.Combine(Application.dataPath, "../_Tools/flatc.exe")
        };

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
            {
                Debug.Log($"找到flatc: {path}");
                return path;
            }
        }

        Debug.Log("未找到flatc编译器");
        return null;
    }

    public bool IsCompilerAvailable(string flatcPath)
    {
        if (string.IsNullOrEmpty(flatcPath))
            return false;

        return File.Exists(flatcPath) || IsCommandAvailable(flatcPath);
    }

    private void ExecuteCommand(string command, string arguments, Action<string> consoleOutput)
    {
        using (var process = new Process())
        {
            consoleOutput?.Invoke($"执行命令: {command} ，参数： {arguments}");

            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                    consoleOutput?.Invoke($"输出: {e.Data}");
                    Debug.Log($"flatc输出: {e.Data}");
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    consoleOutput?.Invoke($"错误: {e.Data}");

                    if (e.Data.ToLower().Contains("error"))
                    {
                        Debug.LogError($"flatc错误: {e.Data}");
                    }
                    else if (e.Data.ToLower().Contains("warning"))
                    {
                        Debug.LogWarning($"flatc警告: {e.Data}");
                    } 
                }
            };

            process.Start();

            // 开始异步读取
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 设置超时
            if (process.WaitForExit(1 * 60 * 1000))
            {
                // 进程正常退出
                consoleOutput?.Invoke($"命令执行成功，退出码: {process.ExitCode}");

                if (process.ExitCode != 0)
                {
                    throw new Exception($"flatc执行失败，退出码: {process.ExitCode}");
                }
            }
            else
            {
                // 超时，杀死进程
                process.Kill();
                throw new Exception($"flatc执行超时，已终止进程");
            }
        }
    }

    private bool IsCommandAvailable(string command)
    {
        try
        {
            using (var process = new Process())
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c where {command}";
                }
                else
                {
                    process.StartInfo.FileName = "/bin/bash";
                    process.StartInfo.Arguments = $"-c \"which {command}\"";
                }

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return !string.IsNullOrEmpty(output) &&
                       !output.Contains("Could not find files") &&
                       !output.Contains("not found");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"检查命令可用性失败: {e.Message}");
            return false;
        }
    }
}