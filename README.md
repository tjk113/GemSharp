# GemSharp
A basic terminal Gemini client, written in C#

## Usage
Build the project using the included Visual Studio 2022 solution. This project targets .NET 7.

Once built, you can provide a URL to a Gemini resource as a command line argument. For example:
```cmd
> .\GemSharp.exe gemini://geminiprotocol.net/
```

## TODO
- [ ] Trust On First Use (TOFU) SSL certificate authentication
- [ ] Viewport Rendering
  - [ ] Limit text bounds to the viewport
  - [ ] Word wrapping
- [ ] Display URL bar below viewport
  - [ ] Display response status on the right-hand side of the URL bar
- [ ] Tab through document links, and press enter to navigate to the current selection