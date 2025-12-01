# nonbon
the skinny kanban

nonbon is a tiny, self-hostable kanban for tracking what you *should* be doing instead of starting another side project. 

nonbon is not Jira, Clickup, or Microsoft Planner. 

It is a digital, post-it note style kanban with no fluff.

It has three simple buckets:

- **Backlog**: things you want to do eventually  
- **Active**: the one or two things you’re focusing on right now  
- **Done**: things you actually finished (before they get archived and forgotten)

It has three main functions:
- .NET API
- HTML/JS front-end
- You can optionally run it in Docker, and it's easier that way :)

Early structure:
```
.                           # Root directory
├── LICENSE                 # Software license
├── README.md               # This document
├── docs                    # Documentation directory
│   └── img                 # Images for Wiki
├── src                     # Source code directory
│   ├── NonBon.Api          # Dotnet API for Nonbon
│   ├── NonBon.Cli          # Dotnet CLI
│   └── NonBon.Web          # Dotnet web application
└── tests                   # xUnit tests
```

## Early instructions (WIP)
### API
Run the API:
```
cd src/NonBon.Api
dotnet run
```

Run the Web UI:
```
cd src/NonBon.Web
dotnet run
```

Open Web UI:
```
http://localhost:5002
```

### CLI
```
# Build the project
dotnet build

# Run the API
dotnet run --project src/NonBon.Api

# In a separate terminal, run the CLI:
dotnet run --project src/NonBon.Cli
```