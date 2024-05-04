using SContainer.Runtime.Unity;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace SContainer.Editor
{
    public sealed class ScriptTemplateModifier : UnityEditor.AssetModificationProcessor
    {
        private const string RootNamespaceBeginTag = "#ROOTNAMESPACEBEGIN#";
        private const string RootNamespaceEndTag = "#ROOTNAMESPACEEND#";

        private const string MonoInstallerTemplate =
            "using SContainer.Runtime;\n" +
            "using SContainer.Runtime.Unity;\n" +
            "\n" +
            RootNamespaceBeginTag + "\n" +
            "public class #SCRIPTNAME# : LifetimeScope\n" +
            "{\n" +
            "    protected override void Configure(IContainerBuilder builder)\n" +
            "    {\n" +
            "    }\n" +
            "}\n" +
            RootNamespaceEndTag + "\n" +
            "";

        public static void OnWillCreateAsset(string metaPath)
        {
            if (SContainerSettings.Instance != null && SContainerSettings.Instance.DisableScriptModifier)
            {
                return;
            }

            var suffixIndex = metaPath.LastIndexOf(".meta");
            if (suffixIndex < 0)
            {
                return;
            }

            var scriptPath = metaPath.Substring(0, suffixIndex);
            var basename = Path.GetFileNameWithoutExtension(scriptPath);
            var extName = Path.GetExtension(scriptPath);
            if (extName != ".cs")
            {
                return;
            }

            if (!scriptPath.EndsWith("LifetimeScope.cs"))
            {
                return;
            }

            var content = MonoInstallerTemplate.Replace("#SCRIPTNAME#", basename);

            var rootNamespace = CompilationPipeline.GetAssemblyRootNamespaceFromScriptPath(scriptPath);
            content = RemoveOrInsertNamespaceSimple(content, rootNamespace);

            if (scriptPath.StartsWith("Assets/"))
            {
                scriptPath = scriptPath.Substring("Assets/".Length);
            }

            var fullPath = Path.Combine(Application.dataPath, scriptPath);
            File.WriteAllText(fullPath, content);
            AssetDatabase.Refresh();
        }
        
        // https://github.com/Unity-Technologies/UnityCsReference/blob/2020.2/Editor/Mono/ProjectWindow/ProjectWindowUtil.cs#L495-L550
        private static string RemoveOrInsertNamespaceSimple(string content, string rootNamespace)
        {
            const char eol = '\n';

            if (string.IsNullOrWhiteSpace(rootNamespace))
            {
                return content
                    .Replace(RootNamespaceBeginTag + eol, "")
                    .Replace(RootNamespaceEndTag + eol, "");
            }

            var lines = content.Split(eol);

            var startAt = ArrayUtility.IndexOf(lines, RootNamespaceBeginTag);
            var endAt = ArrayUtility.IndexOf(lines, RootNamespaceEndTag);

            lines[startAt] = $"namespace {rootNamespace}\n{{";
            {
                for (var i = startAt + 1; i < endAt; ++i)
                {
                    lines[i] = $"    {lines[i]}";
                }
            }
            lines[endAt] = "}";

            return string.Join(eol.ToString(), lines);
        }
    }
}