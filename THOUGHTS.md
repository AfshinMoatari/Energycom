# THOUGHTS.md

---

### Understanding the Requirements

- The main tasks are:
  - How much electricity is produced by our devices?
  - What devices do we have, and what information do we have on them?
  - Any other useful insights that can be derived from the data.

- The solution must be maintainable, easy to continue, and well-documented.

### Data Access

- **EF Core for Data Access:**  
  There are multiple data access methods available in the codebase, including HTTP client calls and direct database access. I chose to use EF Core because it provides a simpler and more flexible data access solution, especially with the ability to use LINQ for queries. Since the console application runs on the backend and has direct access to the database, using EF Core or Dapper is straightforward and efficient.

- **Not Using SamplePeriodicCall or SampleStreamFunction:**  
  I did not use the `SamplePeriodicCall` or `SampleStreamFunction` methods.  
  - `SamplePeriodicCall` uses a polling approach to fetch new data, which is less suitable for a console application where live updates are not easily displayed.
  - `SampleStreamFunction` could not be run due to exceptions in the Ingestion application, specifically related to seeded data where `reading.Meter.Site.Id` is null. Since the Ingestion project is a black box, I ignored this issue and focused on other reliable data access methods.
  - If real-time or decoupled processing was required, a separate infrastructure node (service) could be built to handle streamed data via an open endpoint.

### Data Handling

- **Raw JSON Value Handling:**  
  By examining the JSON values printed by the `SamplePeriodicCall` method, I observed that values can be:
    - Standard numbers (positive/negative)
    - Numbers as strings
    - Hexadecimal or possibly encoded strings
    - Missing or null values

- **Produced vs. Consumed Electricity:**  
  I sum only the positive values as produced electricity, assuming negative values represent consumed electricity (as provided by the reading controller device).

- **Skipped Values:**  
  Skipped values are counted when a reading's `RawJson` cannot be parsed to extract a valid numeric value.

- **Corrupted/Hexadecimal Data Handling:**  
  The corrupted data for the `Value` field appears to be intentional: the `TimedHostedService.DoWork()` method in the Ingestion project populates the database every 5 seconds, and if `CanHaveUnparsedReadings` and `applyEffect` are true, the decimal value is converted to a 32-character hexadecimal string. I also noticed that many values are missing quotes (invalid JSON for a string), which prevents JSON parsing to decimal. To address this, I implemented a Regex fix to add missing quotes around such values. If the value is a 32-character hex string, it is converted to a byte array, then to four `int`s, then to a `decimal`, and finally to a `double`. If the string is a number, it will attempt to parse it directly. These improvements significantly reduce the risk of skipped values due to parsing errors.

  #### Device Listing: GetAllDevicesAsync

    The `GetAllDevicesAsync` method retrieves all meter records from the database using EF Core, eagerly loading related group, site, and configuration entities. Each record is mapped to the `MeterInfo` model, which provides information for device information throughout the application.

    Each device entry includes:
    - Meter ID and meter number
    - Group name and site name
    - Geographic coordinates (latitude, longitude, altitude)
    - Time zone
    - Configuration details (Might be useless here since the valuse are consumed and relavant to injsetion project)

  ### Refactoring and Separation of Concerns

- **Moving Logic Out of Program.cs:**  
  As part of making the application maintainable and extensible, I refactored the code to ensure that `Program.cs` is only responsible for application startup and dependency injection configuration. All business logic, data access, and application flow have been moved into dedicated service and hosted service classes (such as `EnergyAnalysisService` and `ConsoleApp`). This separation of concerns makes the codebase easier to test, reason about, and extend.

    Additionally, I introduced a repository layer (ReadingRepository) to encapsulate all data access logic. By abstracting database operations behind repository interfaces, the service layer remains focused on business rules and orchestration, while data retrieval and persistence are handled separately. This approach improves maintainability, testability, and reusability across the application

  ### Testing Strategy

- **Purpose of the Test Project:**  
  The test project is designed to ensure the correctness and robustness of the core logic in `ReadingParser` and `EnergyCalculator`. These components are critical for accurate data extraction and aggregation, so comprehensive unit tests are essential.

- **ReadingParser Tests:**  
  - Tests cover a variety of input formats, including standard numbers, numbers as strings, 32-character hexadecimal strings, missing or null values, and intentionally malformed JSON.
  - Special attention is given to edge cases, such as missing quotes or corrupted data, to ensure the parser is resilient and minimizes skipped readings.
  - The goal is to verify that all valid values are parsed correctly and that invalid or unparseable values are handled gracefully.

- **EnergyCalculator Tests:**  
  - Tests validate the calculation of produced, consumed, and net electricity from collections of readings.
  - Scenarios include all-positive, all-negative, mixed, and empty input sets, as well as cases with skipped/invalid readings.
  - The focus is on ensuring that aggregation logic matches business rules and that skipped readings are counted and reported accurately.
