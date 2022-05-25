// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Models
{
    using System.Collections.Generic;

    public class ProblemDetailsWithExceptionDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public string TraceId { get; set; }

        public List<ExceptionDetail> ExceptionDetails { get; set; }

        public class ExceptionDetail
        {
            public string Message { get; set; }
            public string Type { get; set; }
            public string Raw { get; set; }
            public List<StackFrame> StackFrames { get; set; }
        }

        public class StackFrame
        {
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public string Function { get; set; }
            public int? Line { get; set; }
            public int? PreContextLine { get; set; }
            public List<string> PreContextCode { get; set; }
            public List<string> ContextCode { get; set; }
            public List<string> PostContextCode { get; set; }
        }
    }
}
