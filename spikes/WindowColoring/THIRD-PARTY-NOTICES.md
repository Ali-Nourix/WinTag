# Third-party components — isolated coloring Spike

Microsoft.Windows.CsWin32 0.3.367 is a build-time generator, not a shipped runtime generator.
Its generated interop is subject to the following upstream license. Source and generator
notices: https://github.com/microsoft/CsWin32/tree/6f3334302499fcbfea98ec3b433da1b2c49bfb47
and https://github.com/microsoft/CsWin32/blob/6f3334302499fcbfea98ec3b433da1b2c49bfb47/NOTICE.txt.

The binary package contains a .NET Windows apphost, not the .NET Desktop Runtime. Apphost
license and SDK third-party notices are copied into that package as DOTNET-LICENSE.txt and
DOTNET-THIRD-PARTY-NOTICES.txt. The user's installed Windows/.NET runtime has its own terms.
Unused Windows SDK.NET/WinRT runtime references are disabled; those DLLs are not distributed.
This notice does not select a license for the user's WinTag project.

## CsWin32 license

The MIT License (MIT)

Copyright (c) Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
