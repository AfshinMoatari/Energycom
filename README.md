# Installation & setup instructions
### Pre-requisites

- install [dotnet 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) 
- install [docker](https://www.docker.com/) 



Navigate to the /src folder

first install the aspire workload

```bash
    dotnet workload install aspire
```

then trust the local dev cert

```bash
    dotnet dev-certs https -t
```

finally you can run the entire solution with 

```bash
    dotnet run --project Energycom.AppHost/Energycom.AppHost.csproj
```

or by running the apphost project with your IDE of choice (VSCode, Rider, VisualStudio etc). If using jetbrains rider you may like the [aspire plugin](https://plugins.jetbrains.com/plugin/23289--net-aspire) which has many useful features such as restarting individual projects without the apphost, auto adding the database to your databases window whilst running etc. 

---

**Note:**  
To view the results for Task #1 to #3, open the console relevant to the Analysis project from the dashboard after starting the solution.
