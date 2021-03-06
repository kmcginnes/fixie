﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fixie
{
    public class TeamCityListener : Listener
    {
        public void AssemblyStarted(Assembly assembly)
        {
            Message("testSuiteStarted name='{0}'", assembly.FileName());
        }

        public void CasePassed(Case @case)
        {
            var durationInMilliseconds = ((int)Math.Ceiling(@case.Duration.TotalMilliseconds)).ToString();
            Message("testStarted name='{0}'", @case.Name);
            Message("testFinished name='{0}' duration='{1}'", @case.Name, durationInMilliseconds);
        }

        public void CaseFailed(Case @case, Exception[] exceptions)
        {
            Message("testStarted name='{0}'", @case.Name);
            Message("testFailed name='{0}' details='{1}'", @case.Name, CompoundStackTrace(exceptions));
            Message("testFinished name='{0}'", @case.Name);
        }

        public void AssemblyCompleted(Assembly assembly, Result result)
        {
            Message("testSuiteFinished name='{0}'", assembly.FileName());
        }

        static void Message(string format, params string[] args)
        {
            var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
            Console.WriteLine("##teamcity["+format+"]", encodedArgs);
        }

        static string Encode(string value)
        {
            var builder = new StringBuilder();
            
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '|': builder.Append("||"); break;
                    case '\'': builder.Append("|'"); break;
                    case '[': builder.Append("|["); break;
                    case ']': builder.Append("|]"); break;
                    case '\n': builder.Append("|n"); break; // Line Feed
                    case '\r': builder.Append("|r"); break; // Carriage Return
                    case '\u0085': builder.Append("|x"); break;  // Next Line
                    case '\u2028': builder.Append("|l"); break;  // Line Separator
                    case '\u2029': builder.Append("|p"); break;  // Paragraph Separator
                    default: builder.Append(ch); break;
                }
            }

            return builder.ToString();
        }

        static string CompoundStackTrace(IEnumerable<Exception> exceptions)
        {
            using (var writer = new StringWriter())
            {
                writer.WriteCompoundStackTrace(exceptions);
                return writer.ToString();
            }
        }
    }
}