// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Models
{
    public class ProblemDetailsWithExceptionDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public string TraceId { get; set; } = default!;

        public List<ExceptionDetail> ExceptionDetails { get; set; } = new();

        public class ExceptionDetail
        {
            public string Message { get; set; } = default!;
            public string Type { get; set; } = default!;
            public string Raw { get; set; } = default!;
            public List<StackFrame> StackFrames { get; set; } = new();
        }

        public class StackFrame
        {
            public string FilePath { get; set; } = default!;
            public string FileName { get; set; } = default!;
            public string Function { get; set; } = default!;
            public int? Line { get; set; }
            public int? PreContextLine { get; set; }
            public List<string> PreContextCode { get; set; } = new();
            public List<string> ContextCode { get; set; } = new();
            public List<string> PostContextCode { get; set; } = new();
        }
    }
}
