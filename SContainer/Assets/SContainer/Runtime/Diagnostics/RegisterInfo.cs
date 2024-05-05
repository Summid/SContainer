using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;

namespace SContainer.Runtime.Diagnostics
{
    public sealed class RegisterInfo
    {
        private static bool displayFileNames = true;
        private static int idSeed;
        
        public int Id { get; }
        public RegistrationBuilder RegistrationBuilder { get; }
        public StackTrace StackTrace { get; }

        private StackFrame headLineStackFrame;

        internal string formattedStackTrace = default; // cache field for internal use(Unity Editor, etc...)

        public RegisterInfo(RegistrationBuilder registrationBuilder)
        {
            this.Id = Interlocked.Increment(ref idSeed);
            this.RegistrationBuilder = registrationBuilder;
            this.StackTrace = new StackTrace(true);
            this.headLineStackFrame = this.GetHeadlineFrame(this.StackTrace);
        }

        public string GetFilename()
        {
            if (this.headLineStackFrame != null && displayFileNames && this.headLineStackFrame.GetILOffset() != StackFrame.OFFSET_UNKNOWN)
            {
                try
                {
                    return this.headLineStackFrame.GetFileName();
                }
                catch (NotSupportedException)
                {
                    displayFileNames = false;
                }
                catch (SecurityException)
                {
                    displayFileNames = false;
                }
            }
            return null;
        }

        public int GetFileLineNumber()
        {
            if (this.headLineStackFrame != null && displayFileNames && this.headLineStackFrame.GetILOffset() != StackFrame.OFFSET_UNKNOWN)
            {
                try
                {
                    return this.headLineStackFrame.GetFileLineNumber();
                }
                catch (NotSupportedException)
                {
                    displayFileNames = false;
                }
                catch (SecurityException)
                {
                    displayFileNames = false;
                }
            }
            return -1;
        }

        public string GetScriptAssetPath()
        {
            var filename = this.GetFilename();
            if (filename == null)
                return "";
            var prefixIndex = filename.LastIndexOf("Assets/");
            return prefixIndex > 0 ? filename.Substring(prefixIndex) : "";
        }

        public string GetHeadline()
        {
            if (this.headLineStackFrame == null)
                return "";

            var method = this.headLineStackFrame.GetMethod();
            var filename = this.GetFilename();
            if (filename != null)
            {
                var lineNumber = this.GetFileLineNumber();
                return $"{method.DeclaringType?.FullName}.{method.Name} (at {Path.GetFileName(filename)}:{lineNumber})";
            }

            var ilOffset = this.headLineStackFrame.GetILOffset();
            if (ilOffset != StackFrame.OFFSET_UNKNOWN)
            {
                return $"{method.DeclaringType?.FullName}.{method.Name}(offset: {ilOffset})";
            }
            return $"{method.DeclaringType?.FullName}.{method.Name}";
        }

        private StackFrame GetHeadlineFrame(StackTrace stackTrace)
        {
            for (var i = 0; i < stackTrace.FrameCount; i++)
            {
                var sf = stackTrace.GetFrame(i);
                if (sf == null) continue;

                var m = sf.GetMethod();
                if (m == null) continue;

                if (m.DeclaringType == null) continue;
                if (m.DeclaringType.Namespace == null || !m.DeclaringType.Namespace.StartsWith("SContainer"))
                {
                    return sf;
                }
            }
            return stackTrace.FrameCount > 0 ? stackTrace.GetFrame(0) : null;
        }
    }
}