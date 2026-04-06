using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace MiniGameSDK.Editor
{
    public static class SDKBuildTool
    {
        [MenuItem("小游戏/打包/打包抖音")]
        public static void BuildDouyin()
        {
            SDKPlatformSwitcher.ToDouyin();
            Build("Douyin");
        }

        [MenuItem("小游戏/打包/打包微信")]
        public static void BuildWechat()
        {
            SDKPlatformSwitcher.ToWechat();
            Build("Wechat");
        }

        [MenuItem("小游戏/打包/打包快手")]
        public static void BuildKuaishou()
        {
            SDKPlatformSwitcher.ToKuaishou();
            Build("Kuaishou");
        }

        [MenuItem("小游戏/打包/打包B站")]
        public static void BuildBili()
        {
            SDKPlatformSwitcher.ToBilibili();
            Build("Bilibili");
        }

        [MenuItem("小游戏/打包/打包支付宝")]
        public static void BuildAlipay()
        {
            SDKPlatformSwitcher.ToAlipay();
            Build("Alipay");
        }

        private static void Build(string dir)
        {
            string path = Path.Combine("Build_MiniGame", dir);
            if (Directory.Exists(path)) Directory.Delete(path, true);

            var op = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path).ToArray(),
                locationPathName = path,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(op);
            EditorUtility.RevealInFinder(path);
        }
    }
}