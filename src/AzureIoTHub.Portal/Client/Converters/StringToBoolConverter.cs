// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Converters
{
    using MudBlazor;

    public class StringToBoolConverter : BoolConverter<string>
    {
        public StringToBoolConverter()
        {
            SetFunc = OnSet;
            GetFunc = OnGet;
        }

        private string OnGet(bool? value) => value?.ToString()?.ToLowerInvariant();

        private bool? OnSet(string arg)
        {
            if (!bool.TryParse(arg, out var value))
            {
                return null;
            }

            return value;
        }
    }
}
