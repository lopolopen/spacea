<img src="https://github.com/lopolopen/spacea-app/blob/main/src/images/logo.png" alt="SpaceA" style="width:100px"/>

# SpaceA
SpaceA = A Space of Agile

## Getting Started
You can run the below commands from the **/src/** directory and get started with the `spacea-api` immediately.
```bash
docker-compose build
docker-compose up
```

## Migration cmd
```bash
dotnet ef migrations add WhatChanges
dotnet ef database update
dotnet ef migrations remove
dotnet ef migrations script
```

## Related Project
https://github.com/lopolopen/spacea-app
