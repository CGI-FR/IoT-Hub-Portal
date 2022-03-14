// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Converters
{
    using System;
    using MudBlazor;

    public class StringToBoolConverter : BoolConverter<string>
    {
        public StringToBoolConverter()
        {
            SetFunc = OnSet;
            GetFunc = OnGet;
        }

        public const string TrueString = "true";
        public const string FalseString = "false";
        public const string NullString = null;

        private string OnGet(bool? value) => value == null ? NullString : (value == true ? TrueString : FalseString);

        private bool? OnSet(string arg)
        {
            try
            {
                if (arg == TrueString)
                    return true;
                if (arg == FalseString)
                    return false;
                return null;
            }
            catch (FormatException e)
            {
                UpdateSetError("Conversion error: " + e.Message);
                return null;
            }
        }
    }
}
