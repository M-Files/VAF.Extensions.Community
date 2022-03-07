# Scheduled Execution

Creation of a `Schdeule` object, and associated triggers, can be used to define when a background operation should be run.  Multiple triggers can be combined to express more complex combinations (e.g. "Weekdays at 8pm, and weekends at 4pm").

### Schedule.cs

The class denoting a schedule.  Contains a list of triggers.

### DailyTrigger.cs

A trigger that runs every day at one or more times.

### WeeklyTrigger.cs

A trigger that runs on one or more days of the week, at one or more times (the same times every day).

### DayOfMonthTrigger.cs

A trigger that runs on a specific numbered day of the month (e.g. every 1st of the month), at one or more times (the same times every day).
