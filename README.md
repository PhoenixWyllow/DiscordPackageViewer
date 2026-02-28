# Discord Package Viewer

A client-side Blazor WebAssembly app for exploring your Discord data export package. Everything runs locally in your browser — **no data is ever sent anywhere**.

This project is open source under the BSD 3-Clause License. Contributions and feedback are welcome!

It is not affiliated with or endorsed by Discord or any affiliated entities. All trademarks are the property of their respective owners.

## Features

- **100% Client-Side** — Your data never leaves your browser. A strict Content Security Policy blocks all outbound network requests.
- **ZIP Upload** — Drag & drop or select your Discord data package `.zip` file directly.
- **Dashboard** — Get an at-a-glance overview of your Discord data with stats and charts.
- **Messages** — Browse your messages by channel with a searchable sidebar.
- **Account** — View your profile, connections, relationships, sessions, and game activity.
- **Servers** — Explore the servers included in your data export.
- **Activity** — Review your Discord activity and analytics events.
- **Billing** — Inspect billing and payment history records.
- **Ads** — See ad demographics, targeting tags, and interest groups Discord has associated with you.
- **Support Tickets** — Read any support tickets included in the export.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

### Run Locally

```bash
cd DiscordPackageViewer
dotnet run
```

Then open the URL shown in the terminal (typically `https://localhost:5001` or `http://localhost:5000`).

### Publish

```bash
dotnet publish DiscordPackageViewer/DiscordPackageViewer.csproj -c Release -o release
```

The static site output will be in `release/wwwroot/`.

## How to Get Your Discord Data Package

1. Open **Discord** → **User Settings** → **Data & Privacy**.
2. Scroll down and click **Request all of my Data**.
3. Discord will email you a download link when your package is ready (this can take up to 30 days).
4. Download the `.zip` file and load it into this app.

## Privacy

This application is designed with absolute data privacy as its core principle:

- **Zero network requests** — CSP blocks all outbound connections.
- **Zero data storage** — Nothing is persisted to disk, local storage, or cookies.
- **Zero telemetry** — No analytics, tracking, or error reporting.
- All processing happens entirely in-browser using WebAssembly.

## Tech Stack

- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) (.NET 10)
- Vanilla CSS (Discord-inspired dark theme)
- GitHub Pages for hosting

## License

This project is licensed under the [BSD 3-Clause License](LICENSE).
