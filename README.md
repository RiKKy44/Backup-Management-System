# Real-Time File Backup System

A lightweight console application written in C# for monitoring and mirroring directories in real-time. The system detects file changes, creations, deletions, and renames, automatically propagating them to a backup location.

## Getting Started

To run the application, you need the .NET SDK installed. Open the terminal in the project directory and execute the following commands:

```bash
dotnet build
dotnet run -c Release
```


## Usage

The application operates via a command-line interface. Below are the available commands:

```bash
add [source_path] [target_path]
```
Starts a background job that monitors the source directory. Any changes made to files in the source are immediately replicated to the target directory.
```bash
restore [backup_path] [original_path]
```
Copies files from the backup directory back to the source location. This process checks whether folders share the same failes to skip redundant operations.
```bash
list
```
Displays all currently active backup jobs and their paths.
```bash
end [source_path] [target_path]
```
Stops the monitoring process for the specified directory pair.

exit
Safely terminates all background threads and closes the application.

## Technical Overview

**System Architecture**
The application is structured around three core components:
* **CommandParser:** Handles user input using Regex to accurately parse complex file paths, including those with spaces and quotes.
* **BackupManager:** Acts as the central controller, maintaining the state of active jobs and managing the lifecycle of background threads. It ensures thread-safe access to the shared job list using lock synchronization.
* **BackupJob:** Encapsulates the logic for a single backup task. It utilizes `FileSystemWatcher` to listen for `Created`, `Changed`, `Renamed`, and `Deleted` events, triggering immediate synchronization with the target directory.

**Concurrency**
To ensure the console interface remains responsive, all backup operations are given to background threads using `Task.Run`. Since multiple jobs can generate logs simultaneously, the `Logger` class implements a thread-safe writing mechanism using a simple static lock object. This prevents race conditions and ensures log messages do not overlap or corrupt the console output.

**Resilience & Error Handling**
File systems often lock files briefly during write operations (e.g., when saving a Word document). To prevent crashes, the system implements a robust retry policy. If an I/O exception occurs during a file copy, the application enters a retry loop, pausing execution for 100ms between attempts up to a defined limit before logging a failure. This ensures high stability during real-time synchronization.

