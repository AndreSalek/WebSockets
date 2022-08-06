using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//importing web socket library
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Server
{
    public class ServerConnect : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Console.WriteLine("New client connected.");
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine("Error \n" + e.Message);
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                //if data is in hex then it gets executed as file at runtime and sends output back to client
                if (isHex(e.Data))
                {
                    byte[] data = FromHex(e.Data);              
                    var fileData = Encoding.ASCII.GetString(data);
                    string output = Compile(fileData);
                    Console.WriteLine(output);
                    Send(output);
                }
                else
                {
                    //if the data is string then application just responds with the string back
                    Send(e.Data);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }
        //return true if string is hexadecimal
        private bool isHex(string data)
        {
            data = data.Replace("-", "");       
            foreach (char c in data)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))) return false;
            }
            return true;
        }

        //takes hexadecimal data and returns byte array
        public byte[] FromHex(string data)
        {
            data = data.Replace("-", "");
            byte[] raw = new byte[data.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(data.Substring(i * 2, 2), 16);
            }
            return raw;
        }
        //method to compile code sent from client at runtime
        public string Compile(string source)
        {
            string runtimePath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1\{0}.dll";
            CSharpCompilationOptions DefaultOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            IEnumerable<MetadataReference> DefaultReferences = new[]
            {
                MetadataReference.CreateFromFile(string.Format(runtimePath, "mscorlib")),
                MetadataReference.CreateFromFile(string.Format(runtimePath, "System")),
                MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Core"))
            };
            static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
            {
                var stringText = SourceText.From(text, Encoding.UTF8);
                return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
            }

            var parsedSyntaxTree = Parse(source, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp5));

            var compilation = CSharpCompilation.Create("clientcode.dll", new SyntaxTree[] { parsedSyntaxTree }, DefaultReferences, DefaultOptions);
            try
            {
                var result = compilation.Emit(@"c:\temp\clientcode.dll");
                var output = result.Success ? "Success!!" : "Failed";
                if (compilation.GetDiagnostics().Any())
                {
                    var errors = compilation.GetDiagnostics().ToList();
                    foreach (var diag in errors)
                        output += $"\n {diag.ToString()}";
                    Console.WriteLine(output);
                }
                
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Error while executing file";
            }
        }

    
    }
}
