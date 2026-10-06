# Release Manager

<!-- release-manager:download -->
[![Download Release Manager 1.0.4](https://img.shields.io/badge/Download-v1.0.4-005FB8?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/jean-knapp/release-manager/releases/download/v1.0.4/ReleaseManager-win-Setup.exe)

[ReleaseManager-win-Setup.exe](https://github.com/jean-knapp/release-manager/releases/download/v1.0.4/ReleaseManager-win-Setup.exe) · Windows installer, version 1.0.4
<!-- /release-manager:download -->

<!-- release-manager:about -->
Release Manager is a Windows desktop tool that turns a C# WinForms/WPF project into a published release on GitHub with as few manual steps as possible. It builds the project in Release mode, optionally obfuscates the output, packages it with Velopack, and publishes the result to GitHub Releases, keeping track of progress for each project across restarts.

## Features

- Builds a .csproj in Release configuration, strips debug symbols and XML documentation, and reports the build's file count and size.
- Can obfuscate the build with the bundled ConfuserEx, with a choice of protections such as renaming, control flow scrambling, string encryption and anti-debug/anti-dump measures.
- Packages the build into a Velopack installer, producing full and delta update packages and reusing the previous release to keep updates small.
- Uploads the package to GitHub Releases, creating the release and tag and using a saved token, an environment variable, or git's own GitHub credentials.
- Adds self-update code to the project automatically: a Velopack hook in Main and an AppUpdater class that checks GitHub for new releases, optionally signed in for a private repository.
- Writes download and SmartScreen instructions into the release description, and keeps the repository's README download button and LICENSE file up to date after each publish.
- Can ask the Claude Code CLI to read the project and draft the README's description section.
- Tracks multiple projects as tabs, remembering each one's settings, release state and recent projects between sessions.
<!-- /release-manager:about -->
